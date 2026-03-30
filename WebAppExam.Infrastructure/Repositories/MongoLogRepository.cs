using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Infrastructure.Repositories;

public class MongoLogRepository : ILogRepository
{
    private readonly IMongoCollection<LogEntry> _logsCollection;

    public MongoLogRepository(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDb:ConnectionString"];

        var databaseName = configuration["MongoDb:DatabaseName"];

        var mongoClient = new MongoClient(connectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseName);

        _logsCollection = mongoDatabase.GetCollection<LogEntry>("Logs");
    }

    public async Task AddAsync(LogEntry logEntry)
    {
        await _logsCollection.InsertOneAsync(logEntry);
    }
}