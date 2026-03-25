using MassTransit;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace WebAppExam.Application.Events
{
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly IMongoClient _mongoClient;

        public OrderCreatedConsumer(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var message = context.Message;

            var db = _mongoClient.GetDatabase("logs_db");
            var collection = db.GetCollection<OrderLog>("order_logs");

            var log = new OrderLog
            {
                OrderId = message.OrderId.ToString(),
                CustomerId = message.CustomerId.ToString(),
                CreatedAt = message.CreatedAt,
                Message = "Order created"
            };

            await collection.InsertOneAsync(log);
        }
    }

    public class OrderLog
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
