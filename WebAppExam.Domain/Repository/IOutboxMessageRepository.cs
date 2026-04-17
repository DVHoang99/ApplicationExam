using System;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Domain.Repository;

public interface IOutboxMessageRepository : IRepository<OutboxMessage>
{
    Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize, DateTime olderThan, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Ulid id, OutboxMessageStatus status, string? error = null, bool? isPermanentFailure = null, int? retryCount = null, CancellationToken cancellationToken = default);
}
