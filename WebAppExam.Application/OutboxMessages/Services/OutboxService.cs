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
using WebAppExam.Domain.Exceptions;
using WebAppExam.Application.OutboxMessages.DTOs;
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
    /// Publishes a message to Kafka and updates the outbox record status.
    /// This method is intended to be called within a Hangfire background job.
    /// It ensures that success/failure is recorded in the database.
    /// </summary>
    public async Task PublishMessageAsync(Ulid outboxMessageId, string kafkaKey, object message, CancellationToken cancellationToken = default)
    {
        // CLAIM CHECK PATTERN: For Order events, we send an OutboxPointer instead of the full payload.
        object messageToSend = message;
        bool isOrderEvent = message is OrderItemProcessedEvent || 
                            message is OrderUpdatedEvent || 
                            message is OrderDeletedEvent || 
                            message is OrderCanceledEvent || 
                            message is OrderCreatedIntegrationEvent;

        if (isOrderEvent)
        {
            messageToSend = new OutboxPointer(outboxMessageId.ToString(), message.GetType().Name);
        }

        bool kafkaSuccess = false;
        string kafkaError = string.Empty;

        // 1. KAFKA RETRY LOOP (Manual loop to separate from DB errors)
        int maxRetries = 3;
        for (int retryCount = 1; retryCount <= maxRetries; retryCount++)
        {
            try
            {
                var producer = _producerAccessor.GetProducer(Constants.KafkaProducer.OrderProducer);
                await producer.ProduceAsync(kafkaKey, messageToSend);
                kafkaSuccess = true;
                break;
            }
            catch (Exception ex)
            {
                kafkaError = ex.Message;
                bool isPermanent = ex is PermanentOutboxException || ex is JsonException || ex is ArgumentException;

                if (isPermanent || retryCount == maxRetries)
                {
                    _logger.LogError(ex, "Outbox Service: Kafka failed {Type} for message {Id} after {Attempt} attempts.", 
                        isPermanent ? "PERMANENTLY" : "FINAL", outboxMessageId, retryCount);
                    break;
                }

                _logger.LogWarning(ex, "Outbox Service: Kafka attempt {Attempt} failed for message {Id}. Retrying...", retryCount, outboxMessageId);
                await Task.Delay(1000 * retryCount, cancellationToken);
            }
        }

        // 2. DATABASE UPDATE (Isolated - No Rethrow to stop Hangfire retry)
        try
        {
            if (kafkaSuccess)
            {
                await _outboxMessageRepository.UpdateStatusAsync(outboxMessageId, OutboxMessageStatus.Sent);
                _logger.LogInformation("Outbox Service: Message {Id} sent to Kafka and status updated to Sent.", outboxMessageId);
            }
            else
            {
                // Kafka failed after all retries or permanent error
                await _outboxMessageRepository.UpdateStatusAsync(
                    outboxMessageId, 
                    OutboxMessageStatus.Failed, 
                    $"Kafka Error: {kafkaError}", 
                    isPermanentFailure: true);
                
                _logger.LogCritical("Outbox Service: Message {Id} marked as FAILED in DB after Kafka failures.", outboxMessageId);
            }
        }
        catch (Exception ex)
        {
            // DATABASE ERROR: We do NOT rethrow here as requested.
            // This prevents Hangfire from retrying and sending duplicate messages to Kafka.
            // The "Safe Net" polling job will find the message remains 'Pending' and handle it later.
            _logger.LogCritical(ex, "Outbox Service: CRITICAL - Kafka send (Success: {KafkaSuccess}) finished but DB update FAILED for message {Id}. " +
                                   "Message remains Pending to be handled by safety net. This prevents duplicates in Hangfire.", kafkaSuccess, outboxMessageId);
        }
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
