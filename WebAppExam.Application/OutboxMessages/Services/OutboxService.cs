using System;
using WebAppExam.Application.Orders;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.OutboxMessages.Services;

public class OutboxService : IOutboxService
{
    private readonly IOutboxMessageRepository _outboxMessageRepository;

    public OutboxService(IOutboxMessageRepository outboxMessageRepository)
    {
        _outboxMessageRepository = outboxMessageRepository;
    }

    public async Task<OutboxMessage?> GetOutboxMessagePendingByMessageIdAsync(string messagePrefix, string id, CancellationToken cancellationToken = default)
    {
        var messageId = $"{messagePrefix}:{id}";

        var outboxMessage = await _outboxMessageRepository
            .FirstOrDefaultAsync(m => m.MessageId == messageId &&
            m.Status == OutboxMessageStatus.Pending,
            cancellationToken);

        return outboxMessage;
    }
}
