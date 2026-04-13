using System;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Common;

public class InboxService : IInboxService
{
    private readonly IInboxMessageRepository _inboxRepository;

    public InboxService(IInboxMessageRepository inboxRepository)
    {
        _inboxRepository = inboxRepository;
    }

    public async Task<bool> HasBeenProcessedAsync(string messageId, CancellationToken cancellationToken = default)
    {
        return await _inboxRepository.HasBeenProcessedAsync(messageId, cancellationToken);
    }

    public async Task<InboxMessage> CreateInboxMessageAsync(string messageId, string type, string? content = null, CancellationToken cancellationToken = default)
    {
        var inboxMessage = InboxMessage.Create(messageId, type, "Pending", content);
        await _inboxRepository.AddAsync(inboxMessage, cancellationToken);
        return inboxMessage;
    }

    public async Task MarkAsProcessedAsync(string messageId, CancellationToken cancellationToken = default)
    {
        var inboxMessage = await _inboxRepository.FirstOrDefaultAsync(m => m.MessageId == messageId, cancellationToken);
        if (inboxMessage != null)
        {
            inboxMessage.MarkAsProcessed();
            _inboxRepository.Update(inboxMessage);
        }
    }

    public async Task UpdateInboxMessageStatusAsync(string messageId, string status, CancellationToken cancellationToken = default)
    {
        var inboxMessage = await _inboxRepository.FirstOrDefaultAsync(m => m.MessageId == messageId, cancellationToken);
        if (inboxMessage != null)
        {
            inboxMessage.MarkAsProcessed();
            _inboxRepository.Update(inboxMessage);
        }
    }
}
