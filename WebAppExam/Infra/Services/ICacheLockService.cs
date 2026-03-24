using StackExchange.Redis;

namespace WebAppExam.Infra.Services
{
    public interface ICacheLockService
    {
        Task<bool> ExecuteWithLockAsync(string lockKey, TimeSpan expiration);
    }

    public class RedisLockService : ICacheLockService
    {
        private readonly IDatabase _database;

        public RedisLockService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<bool> ExecuteWithLockAsync(string lockKey, TimeSpan expiration)
        {
            string token = Guid.NewGuid().ToString();


            bool isLocked = await _database.StringSetAsync(lockKey, token, expiration, When.NotExists);

            if (!isLocked) return false;

            try
            {
                return true;
            }
            finally
            {
                var luaReleaseScript = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

                await _database.ScriptEvaluateAsync(luaReleaseScript,
                    new RedisKey[] { lockKey },
                    new RedisValue[] { token });
            }
        }
    }
}
