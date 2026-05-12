using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain.Enum;
using Microsoft.Data.SqlClient;

namespace WebAppExam.Infrastructure.Repositories;

public class OutboxMessageRepository : Repository<OutboxMessage>, IOutboxMessageRepository
{
    public OutboxMessageRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize, DateTime olderThan, CancellationToken cancellationToken = default)
    {
        var statusParam = new SqlParameter("@status", (int)Domain.Enum.OutboxMessageStatus.Pending);
        var dateParam = new SqlParameter("@olderThan", olderThan);

        return await FromSqlInterpolated($@"
            SELECT * FROM OutboxMessages 
            WHERE Status = {statusParam} 
              AND CreatedAt < {dateParam} 
            ORDER BY CreatedAt ASC
        ")
        .Take(batchSize)
        .ToListAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Ulid id, OutboxMessageStatus status, string? error = null, bool? isPermanentFailure = null, int? retryCount = null, CancellationToken cancellationToken = default)
    {
        // Use EF Core ExecuteUpdateAsync for a high-performance, single-query update without prior SELECT.
        var query = Query().Where(x => x.Id == id);

        // We use a fixed expression to avoid dynamic lambda complexity which can be finicky in EF Core
        await query.ExecuteUpdateAsync(s => s
            .SetProperty(b => b.Status, status)
            .SetProperty(b => b.ProcessedOn, DateTime.UtcNow)
            .SetProperty(b => b.Error, error)
            .SetProperty(b => b.IsPermanentFailure, b => isPermanentFailure.HasValue ? isPermanentFailure.Value : b.IsPermanentFailure)
            .SetProperty(b => b.RetryCount, b => retryCount.HasValue ? retryCount.Value : b.RetryCount),
            cancellationToken);
    }
}