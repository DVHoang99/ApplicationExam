using System.Text.Json;
using KafkaFlow.Producers;
using MediatR;
using Microsoft.Extensions.Logging;
using WebAppExam.Application.OutboxMessages;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;
using WebAppExam.Domain.Events;
using WebAppExam.Infrastructure.Exceptions;
using WebAppExam.Application.OutboxMessages.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.EventHandlers
{
    public class OrderDeletedEventHandler : INotificationHandler<OrderDeletedEvent>
    {
        private readonly IProducerAccessor _producerAccessor;
        private readonly IOutboxService _outboxService;
        private readonly IOutboxMessageRepository _outboxMessageRepository;
        private readonly ILogger<OrderDeletedEventHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public OrderDeletedEventHandler(IProducerAccessor producerAccessor, IOutboxService outboxService, IOutboxMessageRepository outboxMessageRepository, ILogger<OrderDeletedEventHandler> logger, IUnitOfWork unitOfWork)
        {
            _producerAccessor = producerAccessor;
            _outboxService = outboxService;
            _outboxMessageRepository = outboxMessageRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(OrderDeletedEvent message, CancellationToken cancellationToken)
        {
            var outboxMessage = await _outboxService.GetOutboxMessagePendingByMessageIdAsync(
            Constants.KafkaPrefix.OrderDeletedPrefix,
            message.OrderId,
            cancellationToken);

            if (outboxMessage == null)
            {
                _logger.LogWarning("Outbox message not found or already processed for Order: {OrderId}", message.OrderId);
                return;
            }

            try
            {
                var producer = _producerAccessor.GetProducer(Constants.KafkaProducer.OrderProducer);

                var pointer = new OutboxPointer(outboxMessage.Id.ToString(), nameof(OrderDeletedEvent));

                await producer.ProduceAsync(
                    $"{Constants.KafkaPrefix.OrderDeletedPrefix}:{message.OrderId}",
                    pointer
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to produce Kafka message for Order: {OrderId}. Outbox will remain Pending.", message.OrderId);
            }

        }
    }
}
