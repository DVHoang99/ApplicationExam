using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using WebAppExam.Application.Common.Caching;

namespace WebAppExam.Infrastructure.Common.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;

    public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis)
    {
        _cache = cache;
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedData = await _cache.GetStringAsync(key, cancellationToken);
        return cachedData == null ? default : JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration };
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(data), options, cancellationToken);
    }

    public async Task<T?> GetAsync<T>(string key, Func<Task<T>> functionToObtain, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        var value = await GetAsync<T>(key, cancellationToken);

        if (value == null)
        {
            value = await functionToObtain.Invoke();
            if (value != null)
                await SetAsync(key, value, duration, cancellationToken);
        }

        return value;
    }
    public async Task RemoveByPrefixAsync(string prefix)
    {
        // Quét và xóa toàn bộ Key bắt đầu bằng prefix
        var endpoints = _redis.GetEndPoints();
        var server = _redis.GetServer(endpoints.First());

        var keys = server.Keys(pattern: $"{prefix}*").ToArray();
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(keys);
    }
}