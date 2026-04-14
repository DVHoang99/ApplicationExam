using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Common.Enums;
using ZiggyCreatures.Caching.Fusion;
using WebAppExam.Domain.Common;

namespace WebAppExam.Infrastructure.Common.Caching;

public class RedisCacheService : ICacheService
{
    private readonly Dictionary<RedisDbType, ConnectionMultiplexer> _connections;
    private readonly IFusionCache _fusionCache;

    public RedisCacheService(IConfiguration configuration, IFusionCache fusionCache)
    {
        _fusionCache = fusionCache;
        _connections = new Dictionary<RedisDbType, ConnectionMultiplexer>();

        string? cacheStr = configuration[Constants.ConfigKeys.RedisCacheDb];
        string? sessionStr = configuration[Constants.ConfigKeys.RedisSessionDb];
        string? jobQueueStr = configuration[Constants.ConfigKeys.RedisJobQueueDb];
        string? inboxStr = configuration[Constants.ConfigKeys.RedisInboxDb];

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
        return await _fusionCache.GetOrDefaultAsync<T>(key, default, token: cancellationToken);
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        await _fusionCache.SetAsync(key, data, options => options.SetDuration(expiration), token: cancellationToken);
    }

    public async Task<T?> GetAsync<T>(string key, Func<Task<T>> functionToObtain, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        return await _fusionCache.GetOrSetAsync<T>(
            key, 
            async ct => await functionToObtain(), 
            options => options.SetDuration(duration), 
            token: cancellationToken);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        // FusionCache doesn't natively support wildcard removal easily across L1/L2
        // So we still use the direct Redis connection for the backplane/L2 removal
        // Note: This won't automatically clear L1 (Memory) in other instances 
        // unless you use FusionCache's own eviction methods if available.
        // For standard "RemoveByPrefix", direct Redis is often more reliable.
        
        var cacheConnection = _connections[RedisDbType.Cache];
        var endpoints = cacheConnection.GetEndPoints();
        var server = cacheConnection.GetServer(endpoints.First());
        var db = cacheConnection.GetDatabase();
        var keys = server.Keys(database: db.Database, pattern: $"{prefix}*").ToArray();

        if (keys.Any())
        {
            await db.KeyDeleteAsync(keys);
            
            // To be safe, we can try to evict via FusionCache if we have specific keys
            foreach (var key in keys)
            {
                await _fusionCache.RemoveAsync(key.ToString());
            }
        }
    }
}