using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Common.Enums;

namespace WebAppExam.Infrastructure.Common.Caching;

public class RedisCacheService : ICacheService
{
    private readonly Dictionary<RedisDbType, ConnectionMultiplexer> _connections;

    public RedisCacheService(IConfiguration configuration)
    {
        _connections = new Dictionary<RedisDbType, ConnectionMultiplexer>();

        string? cacheStr = configuration["Redis:CacheDb"];
        string? sessionStr = configuration["Redis:SessionDb"];
        string? jobQueueStr = configuration["Redis:JobQueueDb"];
        string? inboxStr = configuration["Redis:InboxDb"];

        if (!string.IsNullOrEmpty(cacheStr))
            _connections[RedisDbType.Cache] = ConnectionMultiplexer.Connect(cacheStr);

        if (!string.IsNullOrEmpty(sessionStr))
            _connections[RedisDbType.Session] = ConnectionMultiplexer.Connect(sessionStr);

        if (!string.IsNullOrEmpty(jobQueueStr))
            _connections[RedisDbType.JobQueue] = ConnectionMultiplexer.Connect(jobQueueStr);

        if (!string.IsNullOrEmpty(inboxStr))
            _connections[RedisDbType.Inbox] = ConnectionMultiplexer.Connect(inboxStr);
    }

    public IDatabase GetDatabase(RedisDbType dbType)
    {
        if (!_connections.ContainsKey(dbType))
        {
            throw new ArgumentException($"Redis connection for {dbType} is not configured.");
        }

        return _connections[dbType].GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var db = _connections[RedisDbType.Cache].GetDatabase();

        // StringGetAsync returns a RedisValue, not a string
        var cachedData = await db.StringGetAsync(key);

        if (cachedData.IsNullOrEmpty)
            return default;

        // FIX: Explicitly cast the RedisValue to a string to resolve the ambiguity
        return JsonSerializer.Deserialize<T>((string)cachedData!);
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var db = _connections[RedisDbType.Cache].GetDatabase();
        var jsonData = JsonSerializer.Serialize(data);

        // Save to Redis with the specified expiration time
        await db.StringSetAsync(key, jsonData, expiration);
    }

    public async Task<T?> GetAsync<T>(string key, Func<Task<T>> functionToObtain, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        // Try to get from Cache first
        var value = await GetAsync<T>(key, cancellationToken);

        // If cache miss, execute the function to get data from database/API
        if (value == null)
        {
            value = await functionToObtain.Invoke();

            // If the obtained value is valid, save it to Cache
            if (value != null)
            {
                await SetAsync(key, value, duration, cancellationToken);
            }
        }

        return value;
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        // Get the specific ConnectionMultiplexer for Cache
        var cacheConnection = _connections[RedisDbType.Cache];

        var endpoints = cacheConnection.GetEndPoints();
        var server = cacheConnection.GetServer(endpoints.First());

        // Search for keys matching the prefix on the Cache DB
        // Make sure to add the database index if you are using multiple logical DBs on the same server instance
        var db = cacheConnection.GetDatabase();
        var keys = server.Keys(database: db.Database, pattern: $"{prefix}*").ToArray();

        if (keys.Any())
        {
            // Delete all matching keys in a single network call
            await db.KeyDeleteAsync(keys);
        }
    }
}