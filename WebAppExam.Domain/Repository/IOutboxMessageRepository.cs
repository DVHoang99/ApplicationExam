using System;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain.Repository;

public interface IOutboxMessageRepository : IRepository<OutboxMessage>
{
    Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize, DateTime olderThan, CancellationToken cancellationToken = default);
}
