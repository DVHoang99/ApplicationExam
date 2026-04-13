using System;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<OutboxMessage?> GetOutboxMessagePendingByIdKeyAsync(string id, CancellationToken cancellationToken = default)
    {
        var convertedId = Ulid.Parse(id);
        var outboxMessage = await _outboxMessageRepository
            .FirstOrDefaultAsync(m => m.Id == convertedId &&
            m.Status == OutboxMessageStatus.Pending,
            cancellationToken);

        return outboxMessage;
    }

    public async Task HandleFailedMessageAsync(OutboxMessage outboxMessage, string errorDetails, CancellationToken cancellationToken)
    {
        try
        {
            outboxMessage.UpdateStatus(OutboxMessageStatus.Pending, errorDetails);
            _outboxMessageRepository.Update(outboxMessage);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to update outbox message status. Message ID: {outboxMessage.Id}, Error: {errorDetails}, Exception: {ex}");
        }
    }
}
