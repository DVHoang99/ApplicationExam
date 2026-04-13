
using System.Text.Json;
using MediatR;
using WebAppExam.Application.Common.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.BackgroundJobs.Services;

public class OutboxRetryJob : IOutboxRetryJob
{
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<OutboxRetryJob> _logger;

    public OutboxRetryJob(IOutboxMessageRepository outboxMessageRepository, IMediator mediator, ILogger<OutboxRetryJob> logger)
    {
        _outboxMessageRepository = outboxMessageRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("--- HANGFIRE: Started scanning Outbox via Repository ---");
        var oneHourAgo = DateTime.UtcNow.AddMilliseconds(-1);

        var pendingMessages = await _outboxMessageRepository.GetPendingMessagesAsync(100, oneHourAgo);

        if (!pendingMessages.Any())
        {
            _logger.LogInformation("Outbox is clean. No pending messages found.");
            return;
        }
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        foreach (var msg in pendingMessages)
        {
            try
            {
                _logger.LogInformation("Retrying Message ID: {MessageId}", msg.Id);

                Type eventType = EventRegistry.GetEventType(msg.Type);
                object? eventObj = JsonSerializer.Deserialize(msg.Content, eventType, jsonOptions);

                if (eventObj != null)
                {
                    await _mediator.Publish(eventObj);
                }
                _logger.LogInformation("-> SUCCESS: Resent Message ID: {MessageId}", msg.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "-> FAILED: Error resending Message ID: {MessageId}", msg.Id);
                // msg.Status = "Failed";
            }
        }

        _logger.LogInformation("--- HANGFIRE: Completed Outbox scan ---");
    }
}
