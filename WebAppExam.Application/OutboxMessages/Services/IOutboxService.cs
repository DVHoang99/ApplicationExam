using System;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Application.OutboxMessages;

public interface IOutboxService
{
    Task<OutboxMessage?> GetOutboxMessagePendingByMessageIdAsync(string messagePrefix, string id, CancellationToken cancellationToken = default);
}