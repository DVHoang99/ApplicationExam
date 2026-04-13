using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;

namespace WebAppExam.Infrastructure.Repositories;

public class InboxMessageRepository : Repository<InboxMessage>, IInboxMessageRepository
{
    public InboxMessageRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> HasBeenProcessedAsync(string messageId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(m => m.MessageId == messageId && m.Status == "Processed", cancellationToken);
    }
}
