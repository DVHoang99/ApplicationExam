using System;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Common;

public interface IInboxService
{
    Task<bool> HasBeenProcessedAsync(string messageId, CancellationToken cancellationToken = default);
    Task<InboxMessage> CreateInboxMessageAsync(string messageId, string type, string? content = null, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string messageId, CancellationToken cancellationToken = default);
}
