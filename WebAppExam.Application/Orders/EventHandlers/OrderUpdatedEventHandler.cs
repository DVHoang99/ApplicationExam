using System;
using KafkaFlow.Producers;
using MediatR;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Events;

namespace WebAppExam.Application.Orders.EventHandlers;

public class OrderUpdatedEventHandler : INotificationHandler<OrderUpdatedEvent>
{
    private readonly IProducerAccessor _producerAccessor;
    public OrderUpdatedEventHandler(IProducerAccessor producerAccessor)
    {
        _producerAccessor = producerAccessor;
    }

    public async Task Handle(OrderUpdatedEvent message, CancellationToken cancellationToken)
    {
        var producer = _producerAccessor.GetProducer(Constants.KafkaProducer.OrderProducer);

        await producer.ProduceAsync(
            $"{Constants.KafkaPrefix.OrderUpdatePrefix}:{message.OrderId}",
            message
        );
    }
}
