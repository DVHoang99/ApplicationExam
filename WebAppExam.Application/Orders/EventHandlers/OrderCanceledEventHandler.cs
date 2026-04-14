using System.Text.Json;
using KafkaFlow.Producers;
using MediatR;
using Microsoft.Extensions.Logging;
using WebAppExam.Application.OutboxMessages;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;
using WebAppExam.Domain.Events;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Exceptions;

namespace WebAppExam.Application.Orders.EventHandlers;

public class OrderCanceledEventHandler : INotificationHandler<OrderCanceledEvent>
{
    private readonly IProducerAccessor _producerAccessor;
    private readonly IOutboxService _outboxService;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly ILogger<OrderCanceledEventHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public OrderCanceledEventHandler(IProducerAccessor producerAccessor, IOutboxService outboxService, IOutboxMessageRepository outboxMessageRepository, ILogger<OrderCanceledEventHandler> logger, IUnitOfWork unitOfWork)
    {
        _producerAccessor = producerAccessor;
        _outboxService = outboxService;
        _outboxMessageRepository = outboxMessageRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(OrderCanceledEvent message, CancellationToken cancellationToken)
    {
        var outboxMessage = await _outboxService.GetOutboxMessagePendingByMessageIdAsync(
        Constants.KafkaPrefix.OrderCanceledPrefix,
        message.OrderId,
        cancellationToken);

        if (outboxMessage == null)
        {
            _logger.LogWarning("Outbox message not found or already processed for Order: {OrderId}", message.OrderId);
            return;
        }

        var jsonFromDb = outboxMessage.Content;

        var orderEvent = JsonSerializer.Deserialize<OrderCanceledEvent>(jsonFromDb);

        try
        {
            var producer = _producerAccessor.GetProducer(Constants.KafkaProducer.OrderProducer);

            await producer.ProduceAsync(
                $"{Constants.KafkaPrefix.OrderCanceledPrefix}:{message.OrderId}",
                orderEvent
            );

            outboxMessage.UpdateStatus(OutboxMessageStatus.Sent, string.Empty);
            _outboxMessageRepository.Update(outboxMessage);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Confluent.Kafka.KafkaException kafkaEx)
        {
            _logger.LogError(kafkaEx, "Kafka error while producing message for Order: {OrderId}. Reason: {Reason}", message.OrderId, kafkaEx.Error.Reason);
            await _outboxService.HandleFailedMessageAsync(outboxMessage, kafkaEx.Message, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception dbEx) when (dbEx.GetType().Name == "DbUpdateException" || dbEx.GetType().Name == "PostgresException")
        {
            _logger.LogError(dbEx, "Database error updating outbox status for Order: {OrderId}.", message.OrderId);
            throw new WebAppExam.Infrastructure.Exceptions.TransientOperationException("Database commit failed", dbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to produce Kafka message for Order: {OrderId}. Outbox will remain Pending.", message.OrderId);
        }
    }
}
