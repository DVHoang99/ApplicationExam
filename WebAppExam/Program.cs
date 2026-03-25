using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using StackExchange.Redis;
using WebAppExam.Application.Events;
using WebAppExam.Application.Middleware;
using WebAppExam.Domain;
using WebAppExam.Infra;
using WebAppExam.Infra.Behaviors;
using WebAppExam.Infra.Services;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Could not find 'Postgres' connection string in appsettings.json");
        }

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        //var mongoClient = new MongoClient(builder.Configuration.GetSection("Mongo:ConnectionString").Value);
        //builder.Services.AddSingleton<IMongoClient>(mongoClient);

        //builder.Services.AddHangfire(config => config
        //.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        //.UseSimpleAssemblyNameTypeSerializer()
        //.UseRecommendedSerializerSettings()
        //.UsePostgreSqlStorage(options =>
        //{
        //    options.UseNpgsqlConnection(connectionString);
        //}));

        //builder.Services.AddHangfireServer(options =>
        //{
        //    options.WorkerCount = Environment.ProcessorCount * 5;
        //});

        var redisConnectionString = builder.Configuration.GetSection("Redis:ConnectionString").Value;
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString!));

        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });



        //builder.Services.AddMassTransit(x =>
        //{
        //    x.UsingInMemory((context, cfg) =>
        //    {
        //        cfg.ConfigureEndpoints(context);
        //    });

        //    x.AddRider(rider =>
        //    {
        //        // 2. Explicitly register the producer so it can be injected
        //        rider.AddProducer<string, ApplicationLog>("app-logs");

        //        rider.AddConsumer<LogConsumer>();

        //        rider.UsingKafka((context, k) =>
        //        {
        //            k.Host("localhost:9092");

        //            k.TopicEndpoint<string, LogConsumer>("add-log-topic", "log-group", e =>
        //            {
        //                e.ConfigureConsumer<LogConsumer>(context);
        //            });
        //        });
        //    });
        //});

        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        builder.Services.AddScoped<ICacheLockService, RedisLockService>();

        builder.Services.AddMassTransit(x =>
        {
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddProducer<WebAppExam.Application.Events.OrderCreatedEvent>("order-created-topic");

                rider.UsingKafka((context, k) =>
                {
                    var kafkaHost = "localhost:9092";
                    k.Host(kafkaHost);
                });
            });
        });

        var app = builder.Build();
        app.UseMiddleware<AuditLogMiddleware>();
        //app.UseHangfireDashboard("/hangfire");
        app.MapControllers();
        app.Run();
    }
}