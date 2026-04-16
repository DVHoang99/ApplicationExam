using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KafkaFlow.Producers;
using Microsoft.Extensions.Logging;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Application.Services;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;
using WebAppExam.Domain.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.OutboxMessages.Services;

public class OutboxService : IOutboxService
{
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IProducerAccessor _producerAccessor;
    private readonly IHangfireJobService _jobService;
    private readonly ILogger<OutboxService> _logger;

    public OutboxService(
        IOutboxMessageRepository outboxMessageRepository,
        IProducerAccessor producerAccessor,
        IHangfireJobService jobService,
        ILogger<OutboxService> logger)
    {
        _outboxMessageRepository = outboxMessageRepository;
        _producerAccessor = producerAccessor;
        _jobService = jobService;
        _logger = logger;
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
            // Note: We still have this for non-job related handling if needed,
            // but the primary status management is shifted to the OutboxJobFilter.
            await _outboxMessageRepository.UpdateStatusAsync(outboxMessage.Id, OutboxMessageStatus.Pending, errorDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update outbox message status. Message ID: {Id}, Error: {Error}", outboxMessage.Id, errorDetails);
        }
    }

    /// <summary>
    /// Pure Kafka Message Publisher.
    /// This method is decoupled from the database. 
    /// Status updates (Sent/Failed) are handled by the OutboxJobFilter (the "Handler").
    /// </summary>
    public async Task PublishMessageAsync(Ulid outboxMessageId, string kafkaKey, object message, CancellationToken cancellationToken = default)
    {
        // PURE DISPATCH LOGIC
        // Zero database reads or writes here.
        var producer = _producerAccessor.GetProducer(Constants.KafkaProducer.OrderProducer);
        await producer.ProduceAsync(kafkaKey, message);
        
        _logger.LogDebug("Kafka Producer: Message {Id} sent to Kafka successfully.", outboxMessageId);
    }

    public async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        // The safety net periodic job
        var query = _outboxMessageRepository.Query()
            .Where(m => m.Status == OutboxMessageStatus.Pending && 
                        !m.IsPermanentFailure && 
                        m.RetryCount < 5)
            .OrderBy(m => m.CreatedAt)
            .Take(50);

        var pendingMessages = await _outboxMessageRepository.ToListAsync(query, cancellationToken);

        if (pendingMessages == null || !pendingMessages.Any()) return;

        _logger.LogInformation("Polling Job: Found {Count} pending outbox messages. Handing them over to the Background Handler...", pendingMessages.Count);

        foreach (var outboxMessage in pendingMessages)
        {
            try
            {
                object? messageData = ResolveMessageFromOutbox(outboxMessage);

                if (messageData != null)
                {
                    // For the polling job, we ENQUEUE the job so that the 
                    // OutboxJobFilter (Handler) handles the status update consistently.
                    _jobService.Enqueue<IOutboxService>(s => 
                        s.PublishMessageAsync(outboxMessage.Id, outboxMessage.MessageId, messageData, CancellationToken.None));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Polling Job: Failed to enqueue message {Id}. Error: {Msg}", outboxMessage.Id, ex.Message);
            }
        }
    }

    private object? ResolveMessageFromOutbox(OutboxMessage outboxMessage)
    {
        return outboxMessage.Type switch
        {
            nameof(OrderItemProcessedEvent) => JsonSerializer.Deserialize<OrderItemProcessedEvent>(outboxMessage.Content),
            nameof(OrderCreatedEvent) => JsonSerializer.Deserialize<OrderCreatedEvent>(outboxMessage.Content),
            nameof(OrderUpdatedEvent) => JsonSerializer.Deserialize<OrderUpdatedEvent>(outboxMessage.Content),
            nameof(OrderDeletedEvent) => JsonSerializer.Deserialize<OrderDeletedEvent>(outboxMessage.Content),
            nameof(OrderCanceledEvent) => JsonSerializer.Deserialize<OrderCanceledEvent>(outboxMessage.Content),
            _ => null
        };
    }
}
