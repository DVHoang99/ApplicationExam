using System.Threading;
using System.Threading.Tasks;
using WebAppExam.Application.OutboxMessages;

namespace WebAppExam.Infrastructure.Jobs;

/// <summary>
/// Background job that periodically scans for pending outbox messages 
/// and attempts to publish them. This acts as a safety net for messages
/// that failed immediate publication or were caught in race conditions.
/// </summary>
public class ProcessOutboxJob
{
    private readonly IOutboxService _outboxService;

    public ProcessOutboxJob(IOutboxService outboxService)
    {
        _outboxService = outboxService;
    }

    /// <summary>
    /// Executes the outbox processing logic.
    /// Hangfire will call this method based on the recurring schedule.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _outboxService.ProcessPendingMessagesAsync(cancellationToken);
    }
}
