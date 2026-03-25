using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using WebAppExam.Application.Events;
using WebAppExam.Domain.LogViewModel;
using WebAppExam.LogConsumer;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Mongo
        var mongoConn = hostContext.Configuration.GetSection("Mongo:ConnectionString").Value;
        services.AddSingleton<IMongoClient>(new MongoClient(mongoConn));

        // MassTransit with Kafka rider
        services.AddMassTransit(x =>
{
    // lightweight bus (required)
    x.UsingInMemory();

    x.AddRider(rider =>
    {
        rider.AddConsumer<OrderCreatedConsumer>();
        rider.AddConsumer<AuditLogConsumer>();

        rider.UsingKafka((context, k) =>
        {
            var kafkaHost = "localhost:9092";

            k.Host(kafkaHost);

            k.TopicEndpoint<OrderCreatedEvent>(
                "order-created-topic",
                "order-created-group-v3",
                e =>
                {
                    e.UseRawJsonDeserializer(isDefault: true);

                    e.ConfigureConsumer<OrderCreatedConsumer>(context);
                });

            k.TopicEndpoint<AuditLogEvent>(
            "audit-log-topic",
            "audit-log-group",
            e =>
            {
                e.ConfigureConsumer<AuditLogConsumer>(context);
            });
                });
    });
});


    })
    .Build();

await host.RunAsync();
