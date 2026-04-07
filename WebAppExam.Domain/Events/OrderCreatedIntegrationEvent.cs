using System;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Application.Orders.Events;

public class OrderCreatedIntegrationEvent(
        Ulid OrderId,
        decimal Amount,
        DateTime OccurredOn,
        int counter
    ) : IDomainEvent
{
    public Ulid OrderId { get; private set; } = OrderId;
    public decimal Amount { get; private set; } = Amount;
    public DateTime OccurredOn { get; private set; } = OccurredOn;
    public int Counter { get; private set; } = counter;
}
