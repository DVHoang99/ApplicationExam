using System;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain.Events;

public class OrderUpdatedEvent : IDomainEvent
{
    public string OrderId { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItemEvent> Items { get; set; } = new();
}
