using System;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Application.OutboxMessages;

public interface IOutboxService
{
    Task<OutboxMessage?> GetOutboxMessagePendingByMessageIdAsync(string messagePrefix, string id, CancellationToken cancellationToken = default);
    Task<OutboxMessage?> GetOutboxMessagePendingByIdKeyAsync(string id, CancellationToken cancellationToken = default);
    Task HandleFailedMessageAsync(OutboxMessage outboxMessage, string errorDetails, CancellationToken cancellationToken = default);
}