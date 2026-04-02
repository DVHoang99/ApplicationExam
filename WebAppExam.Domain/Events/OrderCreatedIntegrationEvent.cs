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
    public Ulid OrderId { get; set; } = OrderId;
    public decimal Amount { get; set; } = Amount;
    public DateTime OccurredOn { get; set; } = OccurredOn;
    public int Counter { get; set; } = counter;
}
