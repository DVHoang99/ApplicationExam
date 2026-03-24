using MassTransit;
using MongoDB.Driver;
using WebAppExam.Domain;

namespace WebAppExam.Application.Events
{
    public class LogConsumer : IConsumer<ApplicationLog>
    {
        private readonly IMongoCollection<ApplicationLog> _logCollection;

        public LogConsumer(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("DiagnosticsDB");
            _logCollection = database.GetCollection<ApplicationLog>("AppLogs");
        }

        public async Task Consume(ConsumeContext<ApplicationLog> context)
        {
            await _logCollection.InsertOneAsync(context.Message);
        }
    }
}
