using System;

namespace WebAppExam.Application.Orders.Events;

public class OrderCreatedIntegrationEvent(
        Ulid OrderId,
        decimal Amount,
        DateTime OccurredOn
    )
{
    public Ulid OrderId { get; set; } = OrderId;
    public decimal Amount { get; set; } = Amount;
    public DateTime OccurredOn { get; set; } = OccurredOn;
}