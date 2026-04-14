using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebAppExam.Application.Common;

public interface IInboxService
{
    Task<bool> HasBeenProcessedAsync(string messageId, CancellationToken cancellationToken = default);
    Task CreateInboxMessageAsync(string messageId, string type, string? content = null, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string messageId, CancellationToken cancellationToken = default);
    Task UpdateInboxMessageStatusAsync(string messageId, string status, CancellationToken cancellationToken = default);
}
