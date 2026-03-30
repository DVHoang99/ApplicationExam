using System.Reflection;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using WebAppExam.Application.Auth.Services;
using WebAppExam.Application.Behaviors;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Revenue;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Common.Caching;
using WebAppExam.Infrastructure.Exceptions;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;
using WebAppExam.Infrastructure.Repositories;
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
        .WithBrokers(kafkaBrokers)
        .AddProducer(
            "order-events-producer",
            producer => producer
                .DefaultTopic("order-events")
                .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
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

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConfig)
);

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
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IRevenueRepository, RevenueRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();

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

builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();
app.MapControllers();
app.Run();
