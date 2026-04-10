using System;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;
using Microsoft.EntityFrameworkCore;

namespace WebAppExam.Infrastructure.Repositories;

public class OutboxMessageRepository : Repository<OutboxMessage>, IOutboxMessageRepository
{
    public OutboxMessageRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize, DateTime olderThan, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(m => m.Status == Domain.Enum.OutboxMessageStatus.Pending && m.CreatedAt < olderThan)
                            .OrderBy(m => m.CreatedAt)
                            .Take(batchSize)
                            .ToListAsync();
    }
}
