using System;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Application.OutboxMessages;

public interface IOutboxService
{
    Task<OutboxMessage?> GetOutboxMessagePendingByMessageIdAsync(string messagePrefix, string id, CancellationToken cancellationToken = default);
    Task<OutboxMessage?> GetOutboxMessagePendingByIdKeyAsync(string id, CancellationToken cancellationToken = default);
    Task HandleFailedMessageAsync(OutboxMessage outboxMessage, string errorDetails, CancellationToken cancellationToken = default);
    Task PublishMessageAsync(Ulid outboxMessageId, string kafkaKey, object message, CancellationToken cancellationToken = default);
    Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default);
}