using MassTransit;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using WebAppExam.Domain.LogViewModel;

namespace WebAppExam.LogConsumer
{
    public class AuditLogConsumer : IConsumer<AuditLogEvent>
    {
        private readonly IMongoCollection<AuditLogEvent> _collection;

        public AuditLogConsumer(IMongoClient client)
        {
            var database = client.GetDatabase("AuditLogsDb");
            _collection = database.GetCollection<AuditLogEvent>("AuditLogs");
        }

        public async Task Consume(ConsumeContext<AuditLogEvent> context)
        {
            var message = context.Message;

            await _collection.InsertOneAsync(message);

            Console.WriteLine($"Saved audit log: {message.Path}");
        }
    }
}
