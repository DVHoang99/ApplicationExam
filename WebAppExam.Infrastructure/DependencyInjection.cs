using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.PostgreSql;
using KafkaFlow;
using KafkaFlow.Serializer;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Logger.Handlers;
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
using WebAppExam.Infrastructure.Protos;

namespace WebAppExam.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. DATABASE (PostgreSQL)
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // 2. REDIS & CACHING
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

        // 4. EXTERNAL / INTERNAL HTTP CLIENTS
        var inventoryServiceHost = configuration.GetSection("InternalService")["InventoryService"] ?? "http://localhost:5134/";

        services.AddHttpClient<IWareHouseService, WarehouseInternalClient>(client =>
        {
            client.BaseAddress = new Uri(inventoryServiceHost);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<IInventoryService, InventoryInternalClient>(client =>
        {
            client.BaseAddress = new Uri(inventoryServiceHost);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // 5. INFRASTRUCTURE SERVICES
        services.AddScoped<IJobService, HangfireJobService>();
        services.AddScoped<IInventoryReservationService, InventoryReservationService>();

        // 6. HANGFIRE CONFIGURATION
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection")))
            .UseFilter(new AutomaticRetryAttribute { Attempts = 5 })
        );
        services.AddHangfireServer();
        services.AddScoped<InventoryReconciliationJob, InventoryReconciliationJob>();

        // 7. KAFKA CONFIGURATION
        AddKafkaMessaging(services, configuration);

        // 8. GRPC
        var grpcServiceHost = configuration.GetSection("GrpcService")["InventoryService"] ?? "http://localhost:5000/";
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
                .CreateTopicIfNotExists(Constants.KafkaTopic.OrderCreatedTopic, 3, 1)
                .CreateTopicIfNotExists(Constants.KafkaTopic.OrderUpdatedTopic, 3, 1)
                .CreateTopicIfNotExists(Constants.KafkaTopic.OrderDeletedTopic, 3, 1)
                .CreateTopicIfNotExists(Constants.KafkaTopic.OrderCanceledTopic, 3, 1)
                .CreateTopicIfNotExists(Constants.KafkaTopic.OrderTopic, 3, 1)

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
                .AddConsumer(consumer => consumer
                    .Topic(Constants.KafkaTopic.SystemLogsTopic)
                    .WithGroupId(Constants.KafkaGroup.APIInternalLoggerGroup)
                    .WithWorkersCount(2)
                    .WithBufferSize(100)
                    .AddMiddlewares(middlewares => middlewares
                        .AddDeserializer<JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h.AddHandler<LogMessageHandler>())
                    )
                )
                .AddConsumer(consumer => consumer
                    .Topic(Constants.KafkaTopic.OrderReplyTopic)
                    .WithGroupId(Constants.KafkaGroup.OrderReplyTopicGroup)
                    .WithWorkersCount(2)
                    .WithBufferSize(100)
                    .AddMiddlewares(middlewares => middlewares
                        .AddSingleTypeDeserializer<OrderReplyDTO, JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h.AddHandler<OrderReplyHandler>())
                    )
                )
            )
        );
    }
}