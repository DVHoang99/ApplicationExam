using System;

namespace WebAppExam.Application.Events
{
    public record OrderCreatedEvent(Guid OrderId, Guid CustomerId, DateTime CreatedAt)
    {
        // Parameterless ctor required by some serializers/consumers (MassTransit warning)
        public OrderCreatedEvent() : this(default, default, default) { }
    }
}
