using System;
using StackExchange.Redis;
using WebAppExam.Application.Common.Enums;

namespace WebAppExam.Application.Common.Caching;

public interface ICacheService
{
    IDatabase GetDatabase(RedisDbType dbType);
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T data, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(string key, Func<Task<T>> functionToObtain, TimeSpan duration, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix);
}
