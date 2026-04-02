using System;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Events;

namespace WebAppExam.Application.Orders.Events;

public class OrderCreatedEvent : IDomainEvent
{
    public string OrderId { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItemEvent> Items { get; set; } = new();
}