using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KafkaFlow.Producers;
using MediatR;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Events;

namespace WebAppExam.Application.Orders.EventHandlers
{
    public class OrderDeletedEventHandler : INotificationHandler<OrderDeletedEvent>
    {
        private readonly IProducerAccessor _producerAccessor;
        public OrderDeletedEventHandler(IProducerAccessor producerAccessor)
        {
            _producerAccessor = producerAccessor;
        }
        public async Task Handle(OrderDeletedEvent message, CancellationToken cancellationToken)
        {
            var producer = _producerAccessor.GetProducer("order-deleted-producer");

            await producer.ProduceAsync(
                $"{Constants.KafkaPrefix.OrderDeletedPrefix}:{message.OrderId}",
                message
            );
        }
    }
}