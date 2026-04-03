using System.Reflection;
using System.Security.Claims;
using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using WebAppExam.API.Services;
using WebAppExam.Application.Auth.Services;
using WebAppExam.Application.Behaviors;
using WebAppExam.Application.Common;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Logger.Handlers;
using WebAppExam.Application.Orders.Consumers;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Products.Services;
using WebAppExam.Application.Revenue;
using WebAppExam.Application.Services;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Common.Caching;
using WebAppExam.Infrastructure.Exceptions;
using WebAppExam.Infrastructure.Jobs;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;
using WebAppExam.Infrastructure.Repositories;
using WebAppExam.Infrastructure.Services;
using WebAppExam.Infrastructure.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var kafkaBrokers = builder.Configuration.GetSection("KafkaConfig:Brokers").Get<string[]>()
                   ?? new[] { "localhost:9092" };
builder.Services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { "localhost:9092" })
        .CreateTopicIfNotExists("order-created-topic", 3, 1)
        .CreateTopicIfNotExists("order-updated-topic", 3, 1)
        .CreateTopicIfNotExists("order-deleted-topic", 3, 1)
        .AddProducer(
            "order-events-producer",
            producer => producer
                .DefaultTopic("order-events")
                .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )
        .AddProducer(
            "system-logs-producer",
            producer => producer
                .DefaultTopic("system-logs-topic")
                .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )
        .AddProducer(
            "order-producer",
            producer => producer
                    .DefaultTopic("order-created-topic")
                    .AddMiddlewares(middlewares => middlewares.AddSerializer<JsonCoreSerializer>())
        )
        .AddProducer(
            "order-update-producer",
            producer => producer
                    .DefaultTopic("order-updated-topic")
                    .AddMiddlewares(middlewares => middlewares.AddSerializer<JsonCoreSerializer>())
        )
        .AddProducer(
            "order-deleted-producer",
            producer => producer
                    .DefaultTopic("order-deleted-topic")
                    .AddMiddlewares(middlewares => middlewares.AddSerializer<JsonCoreSerializer>())
        )
        .AddConsumer(consumer => consumer
            .Topic("order-events")
            .WithGroupId("revenue-update-group")
            .WithWorkersCount(3)
            .WithBufferSize(100)
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(h => h.AddHandler<RevenueUpdateHandler>())
            )
        )
        .AddConsumer(consumer => consumer
            .Topic("system-logs-topic")
            .WithGroupId("api-internal-logger-group")
            .WithWorkersCount(2)
            .WithBufferSize(100)
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(h => h.AddHandler<LogMessageHandler>())
            )
        )
        .AddConsumer(consumer => consumer
            .Topic("order-reply-topic")
            .WithGroupId("order-reply-topic-group")
            .WithWorkersCount(2)
            .WithBufferSize(100)
            .AddMiddlewares(middlewares => middlewares
                .AddSingleTypeDeserializer<OrderReplyDTO, JsonCoreDeserializer>()
                .AddTypedHandlers(h => h.AddHandler<OrderReplyHandler>())
            )
        )
        .AddConsumer(consumer => consumer
            .Topic("order-updated-reply-topic")
            .WithGroupId("order-reply-topic-group")
            .WithWorkersCount(2)
            .WithBufferSize(100)
            .AddMiddlewares(middlewares => middlewares
                .AddSingleTypeDeserializer<OrderReplyDTO, JsonCoreDeserializer>()
                .AddTypedHandlers(h => h.AddHandler<OrderUpdatedReplyHandler>())
            )
        )
    )
);

var redisConfig = builder.Configuration.GetSection("Redis")["Configuration"] ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConfig)
);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfig;
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly(), typeof(TransactionBehavior<,>).Assembly);

    // Đăng ký Behavior
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));

});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
//builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IRevenueRepository, RevenueRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDailyRevenueRepository, DailyRevenueRepository>();
builder.Services.AddScoped<ICacheLockService, CacheLockService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<ILogRepository, MongoLogRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInventoryService, InventoryInternalClient>();
builder.Services.AddScoped<IJobService, HangfireJobService>();
builder.Services.AddScoped<IInventoryReservationService, InventoryReservationService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

            RoleClaimType = ClaimTypes.Role
        };
    });

// Add authorization services
builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

builder.Services.AddHangfire(config => config
.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
.UseSimpleAssemblyNameTypeSerializer()
.UseRecommendedSerializerSettings()
.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")))
.UseFilter(new AutomaticRetryAttribute { Attempts = 5 })
);

builder.Services.AddHangfireServer();

builder.Services.AddControllers();

builder.Services.AddHttpClient<IWareHouseService, WarehouseInternalClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5134/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IInventoryService, InventoryInternalClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5134/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});



builder.Services.AddScoped<InventoryReconciliationJob, InventoryReconciliationJob>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // recurringJobManager.AddOrUpdate<RevenueBackgroundJob>(
    //     "daily-revenue-calculation",
    //     job => job.RunDailyCalculation(),
    //     "* * * * *"
    // );

    // recurringJobManager.AddOrUpdate<InventoryReconciliationJob>(
    //     "reconcile-pending-products",
    //     job => job.ReconcilePendingProductsAsync(),
    //     "*/05 * * * *"
    // );
}

//app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();
app.MapControllers();
app.Run();
