using System;
using KafkaFlow.Producers;
using MediatR;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Events;

namespace WebAppExam.Application.Orders.EventHandlers;

public class OrderCanceledEventHandler : INotificationHandler<OrderCanceledEvent>
{
    private readonly IProducerAccessor _producerAccessor;
    public OrderCanceledEventHandler(IProducerAccessor producerAccessor)
    {
        _producerAccessor = producerAccessor;
    }

    public async Task Handle(OrderCanceledEvent message, CancellationToken cancellationToken)
    {
        var producer = _producerAccessor.GetProducer("order-canceled-producer");

        await producer.ProduceAsync(
           $"{Constants.KafkaPrefix.OrderCreatedPrefix}:{message.OrderId}",
           message
       );
    }
}
