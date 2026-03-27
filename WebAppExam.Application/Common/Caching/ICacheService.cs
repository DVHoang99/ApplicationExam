using System;

namespace WebAppExam.Application.Common.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T data, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix);
}
