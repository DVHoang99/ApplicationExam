using System;
using KafkaFlow.Producers;
using MediatR;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Common;

namespace WebAppExam.Application.Orders.Commands;

public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IProducerAccessor _producerAccessor;
    public OrderCreatedEventHandler(IProducerAccessor producerAccessor)
    {
        _producerAccessor = producerAccessor;
    }

    public async Task Handle(OrderCreatedEvent message, CancellationToken cancellationToken)
    {
        var producer = _producerAccessor.GetProducer("order-producer");

        await producer.ProduceAsync(
            $"{Constants.KafkaPrefix.OrderCreatedPrefix}:{message.OrderId}",
            message
        );
    }
}
