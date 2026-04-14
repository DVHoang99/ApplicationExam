using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Hangfire;
using KafkaFlow;
using KafkaFlow.Serializer;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.Consumers;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Revenue;
using WebAppExam.Application.Services;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Common.Caching;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;
using WebAppExam.Infrastructure.Repositories;
using WebAppExam.Infrastructure.Services;
using WebAppExam.API.Common.Kafka;
using WebAppExam.Application.Products.Services;
using WebAppExam.Application.Common;
using WebAppExam.GrpcContracts.Protos;
using Hangfire.Redis.StackExchange;
using KafkaFlow.Retry;
using WebAppExam.Infrastructure.Exceptions;
using Confluent.Kafka;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.NewtonsoftJson;

namespace WebAppExam.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. DATABASE (PostgreSQL)
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // 2. REDIS & CACHING
        services.AddSingleton<IConnectionMultiplexer>(sp => 
            ConnectionMultiplexer.Connect(configuration[Constants.ConfigKeys.RedisCacheDb] ?? Constants.ConfigDefaults.LocalRedis));

        services.AddFusionCache()
        .WithOptions(options => {
            // We can't explicitly set the memory cache here in the builder easily in 2.6.0
            // but we can ensure the registration order is correct.
        })
        // 1. DEFAULT ENTRY OPTIONS (MULTI-TIER TTL CONFIGURATION)
        .WithDefaultEntryOptions(new FusionCacheEntryOptions
        {
            // The Time-To-Live (TTL) for the local Memory Cache (RAM / L1).
            // Keeps the data ultra-fast and accessible in-memory for 5 minutes.
            Duration = Constants.CacheDuration.DefaultL1,
            
            // The Time-To-Live (TTL) specifically for the Distributed Cache (Redis / L2).
            // By setting this, Redis will hold the data for 2 hours, even if the RAM cache expires.
            // If left unset, it automatically falls back to the L1 Duration.
            DistributedCacheDuration = Constants.CacheDuration.DefaultL2,
            
            // Cache Stampede (Thundering Herd) prevention.
            // Adds a random jitter (0 to 30 seconds) to the Duration to prevent multiple 
            // cache keys from expiring at the exact same time and overloading the Database.
            JitterMaxDuration = Constants.CacheDuration.Jitter
        })
        // 2. SERIALIZER CONFIGURATION
        // Uses Newtonsoft.Json to serialize C# objects into JSON strings before storing them in Redis.
        .WithSerializer(new FusionCacheNewtonsoftJsonSerializer())
        // 3. BACKPLANE CONFIGURATION (L1 MEMORY CACHE SYNCHRONIZATION)
        // Utilizes Redis Pub/Sub. When a server/instance updates or deletes a key, 
        // it broadcasts a message so other instances automatically evict the stale data from their local RAM.
        .WithBackplane(new RedisBackplane(new RedisBackplaneOptions
        {
            Configuration = configuration[Constants.ConfigKeys.RedisCacheDb] ?? Constants.ConfigDefaults.LocalRedis
        }))
        // 4. DISTRIBUTED CACHE CONFIGURATION (L2 CACHE)
        // Integrates Redis as the Layer 2 cache. The retrieval flow becomes: 
        // L1 (Memory) -> L2 (Redis) -> Database.
        .WithDistributedCache(
            new RedisCache(new RedisCacheOptions() { Configuration = configuration[Constants.ConfigKeys.RedisCacheDb] ?? Constants.ConfigDefaults.LocalRedis })
        );

        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddScoped<ICacheLockService, CacheLockService>();

        // 3. REPOSITORIES & UNIT OF WORK
        services.AddScoped<IUnitOfWork, WebAppExam.Infrastructure.UnitOfWork.UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IRevenueRepository, RevenueRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDailyRevenueRepository, DailyRevenueRepository>();
        services.AddSingleton<ILogRepository, MongoLogRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        // 4. EXTERNAL / INTERNAL HTTP CLIENTS
        var inventoryServiceHost = configuration[Constants.ConfigKeys.InternalInventoryService] ?? Constants.ConfigDefaults.LocalInventoryService;
        var inventoryServiceUri = new Uri(inventoryServiceHost);

        // Register HTTP context accessor for JWT token forwarding
        services.AddHttpContextAccessor();

        // Register HTTP client service first (needed for InventoryInternalClient)
        services.AddScoped<IHttpClientService, HttpClientService>();

        // Register HTTP clients for services that use raw HttpClient
        services.AddHttpClient<IWareHouseService, WarehouseInternalClient>(client =>
        {
            client.BaseAddress = new Uri(inventoryServiceHost);
            client.DefaultRequestHeaders.Add(Constants.HttpHeader.Accept, Constants.HttpHeader.ApplicationJson);
        });

        // Register InventoryInternalClient as scoped service (uses IHttpClientService, not raw HttpClient)
        services.AddScoped<IInventoryService>(sp =>
        {
            var httpClientService = sp.GetRequiredService<IHttpClientService>();
            var grpcClient = sp.GetRequiredService<InventoryGrpc.InventoryGrpcClient>();
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            return new InventoryInternalClient(httpClientService, grpcClient, httpContextAccessor, configuration);
        });

        // 5. INFRASTRUCTURE SERVICES
        services.AddScoped<IJobService, HangfireJobService>();
        services.AddScoped<IHangfireJobService, HangfireJobService>();
        services.AddScoped<IRevenueCalculationService, RevenueCalculationService>();
        services.AddScoped<IHangfireConfigurationService, HangfireConfigurationService>();
        services.AddScoped<IInventoryReservationService, InventoryReservationService>();

        // 6. HANGFIRE CONFIGURATION
        var hangfireDbConnection = configuration[Constants.ConfigKeys.RedisHangfireDb] ?? Constants.ConfigDefaults.LocalHangfireRedis;

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseRedisStorage(hangfireDbConnection, new RedisStorageOptions
            {
                Prefix = Constants.CachePrefix.HangfirePrefix,
                Db = 4
            })
            .UseFilter(new AutomaticRetryAttribute { Attempts = 5 })
        );

        services.AddHangfireServer();
        services.AddScoped<InventoryReconciliationJob, InventoryReconciliationJob>();

        // 7. KAFKA CONFIGURATION
        AddKafkaMessaging(services, configuration);

        // 8. GRPC
        var grpcServiceHost = configuration[Constants.ConfigKeys.GrpcInventoryService] ?? Constants.ConfigDefaults.LocalGrpc;
        var grpcUri = new Uri(grpcServiceHost);

        services.AddGrpcClient<WarehouseGrpc.WarehouseGrpcClient>(o => o.Address = grpcUri);
        services.AddGrpcClient<InventoryGrpc.InventoryGrpcClient>(o => o.Address = grpcUri);

        return services;
    }

    private static void AddKafkaMessaging(IServiceCollection services, IConfiguration configuration)
    {
        var kafkaBrokers = configuration.GetSection("KafkaConfig:Brokers").Get<string[]>() ?? new[] { "localhost:9092" };

        services.AddKafka(kafka => kafka
            .UseConsoleLog()
            .AddCluster(cluster => cluster
                .WithBrokers(kafkaBrokers)

                // --- TOPIC CREATION ---
                .CreateTopicIfNotExists(Constants.KafkaTopic.OrderTopic, 3, 1)
                .CreateTopicIfNotExists(Constants.KafkaTopic.OrderEventsTopic, 3, 1)
                .CreateTopicIfNotExists(Constants.KafkaTopic.SystemLogsTopic, 3, 1)
                .CreateTopicIfNotExists(Constants.KafkaTopic.OrderReplyTopic, 3, 1)

                // --- PRODUCERS ---
                .AddProducer(
                    Constants.KafkaProducer.OrderEventProducer,
                    producer => producer
                        .DefaultTopic(Constants.KafkaTopic.OrderEventsTopic)
                        .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
                )
                .AddProducer(
                    Constants.KafkaProducer.SystemLogsProducer,
                    producer => producer
                        .DefaultTopic(Constants.KafkaTopic.SystemLogsTopic)
                        .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
                )
                .AddProducer(
                    Constants.KafkaProducer.OrderProducer,
                    producer => producer
                        .DefaultTopic(Constants.KafkaTopic.OrderTopic)
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer<JsonCoreSerializer, MessageTypeResolver>()
                        )
                )

                // --- CONSUMERS ---
                .AddConsumer(consumer => consumer
                    .Topic(Constants.KafkaTopic.OrderEventsTopic)
                    .WithGroupId(Constants.KafkaGroup.RevenueUpdateGroup)
                    .WithWorkersCount(3)
                    .WithBufferSize(100)
                    .AddMiddlewares(middlewares => middlewares
                        .AddDeserializer<JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h.AddHandler<RevenueUpdateHandler>())
                    )
                )
                // .AddConsumer(consumer => consumer
                //     .Topic(Constants.KafkaTopic.SystemLogsTopic)
                //     .WithGroupId(Constants.KafkaGroup.APIInternalLoggerGroup)
                //     .WithWorkersCount(2)
                //     .WithBufferSize(100)
                //     .AddMiddlewares(middlewares => middlewares
                //         .AddDeserializer<JsonCoreDeserializer>()
                //         .AddTypedHandlers(h => h.AddHandler<LogMessageHandler>())
                //     )
                // )
                .AddConsumer(consumer => consumer
                    .Topic(Constants.KafkaTopic.OrderReplyTopic)
                    .WithGroupId(Constants.KafkaGroup.OrderReplyTopicGroup)
                    .WithWorkersCount(2)
                    .WithBufferSize(100)
                    .AddMiddlewares(middlewares => middlewares
                        .AddSingleTypeDeserializer<OrderReplyDTO, JsonCoreDeserializer>()
                        .RetrySimple(retry => retry
                            .Handle<TransientOperationException>()
                            .Handle<KafkaException>()
                            .Handle<RedisException>()
                            .Handle<RedisTimeoutException>()
                            .Handle<RedisConnectionException>()
                            .TryTimes(3)
                            .WithTimeBetweenTriesPlan((retryCount) => Constants.KafkaRetry.InfrastructureRetryDelay)
                            .Handle<Exception>(x => x.GetType().Name == "DbUpdateException" || x.GetType().Name == "PostgresException")
                        )
                        .AddTypedHandlers(h => h.AddHandler<OrderReplyHandler>())
                    )
                )
            )
        );
    }
}
