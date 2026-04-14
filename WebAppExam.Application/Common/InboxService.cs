using System;
using System.Threading;
using System.Threading.Tasks;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Common.Enums;
using StackExchange.Redis;

namespace WebAppExam.Application.Common;

public class InboxService : IInboxService
{
    private readonly ICacheService _cacheService;

    public InboxService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<bool> HasBeenProcessedAsync(string messageId, CancellationToken cancellationToken = default)
    {
        var db = _cacheService.GetDatabase(RedisDbType.Inbox);
        var key = $"inbox:{messageId}";
        var value = await db.StringGetAsync(key);
        return value.HasValue && value == "Processed";
    }

    public async Task CreateInboxMessageAsync(string messageId, string type, string? content = null, CancellationToken cancellationToken = default)
    {
        var db = _cacheService.GetDatabase(RedisDbType.Inbox);
        var key = $"inbox:{messageId}";
        await db.StringSetAsync(key, "Pending", TimeSpan.FromDays(1));
    }

    public async Task MarkAsProcessedAsync(string messageId, CancellationToken cancellationToken = default)
    {
        var db = _cacheService.GetDatabase(RedisDbType.Inbox);
        var key = $"inbox:{messageId}";
        await db.StringSetAsync(key, "Processed", TimeSpan.FromDays(1));
    }

    public async Task UpdateInboxMessageStatusAsync(string messageId, string status, CancellationToken cancellationToken = default)
    {
        var db = _cacheService.GetDatabase(RedisDbType.Inbox);
        var key = $"inbox:{messageId}";
        await db.StringSetAsync(key, status, TimeSpan.FromDays(1));
    }
}
