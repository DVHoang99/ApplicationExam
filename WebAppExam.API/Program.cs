using System.Reflection;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Application.Behaviors;
using WebAppExam.Application.Revenue;
using WebAppExam.Domain.Repository;
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
            // THÊM 2 DÒNG NÀY VÀO ĐỂ FIX LỖI:
            .WithWorkersCount(3) // Số luồng xử lý song song
            .WithBufferSize(100) // Kích thước hàng đợi tin nhắn trong RAM
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(h => h.AddHandler<RevenueUpdateHandler>())
            )
        )
    )
);


builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly(), typeof(TransactionBehavior<,>).Assembly);

    // Đăng ký Behavior
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddControllers();

var app = builder.Build();
var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();
app.MapControllers();
app.Run();