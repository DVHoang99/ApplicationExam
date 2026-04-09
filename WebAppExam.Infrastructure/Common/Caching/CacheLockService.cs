using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Common.Enums; // Required for RedisDbType

namespace WebAppExam.Infrastructure.Common.Caching;

public class CacheLockService : ICacheLockService
{
    private readonly IDatabase _redisDb;
    private const int MaxRetry = 3;
    private const int RetryDelayMs = 200;

    // FIX: Inject ICacheService instead of IConnectionMultiplexer
    public CacheLockService(ICacheService cacheService)
    {
        // Get the specific database for Cache (Database 0)
        _redisDb = cacheService.GetDatabase(RedisDbType.Cache);
    }

    public async Task<List<string>> AcquireMultipleLocksAsync(IEnumerable<string> lockKeys, string lockToken, TimeSpan expiry)
    {
        var acquiredKeys = new List<string>();

        foreach (var key in lockKeys)
        {
            bool isLocked = false;

            for (int i = 0; i < MaxRetry; i++)
            {
                if (await _redisDb.LockTakeAsync(key, lockToken, expiry))
                {
                    isLocked = true;
                    break;
                }
                await Task.Delay(RetryDelayMs);
            }

            if (isLocked)
            {
                acquiredKeys.Add(key);
            }
            else
            {
                await ReleaseMultipleLocksAsync(acquiredKeys, lockToken);
                return new List<string>();
            }
        }

        return acquiredKeys;
    }

    public async Task ReleaseMultipleLocksAsync(IEnumerable<string> lockKeys, string lockToken)
    {
        var tasks = lockKeys.Select(key => _redisDb.LockReleaseAsync(key, lockToken));
        await Task.WhenAll(tasks);
    }
}