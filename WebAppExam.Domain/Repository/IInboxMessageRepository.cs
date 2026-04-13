using System;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain.Repository;

public interface IInboxMessageRepository : IRepository<InboxMessage>
{
    Task<bool> HasBeenProcessedAsync(string messageId, CancellationToken cancellationToken = default);
}
