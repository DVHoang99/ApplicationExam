using System;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Events;

namespace WebAppExam.Application.Orders.Events;

public class OrderCreatedEvent : IDomainEvent
{
    public string OrderId { get; private set; }
    public string CustomerName { get; private set; }
    public List<OrderItemEvent> Items { get; private set; }

    private OrderCreatedEvent(string orderId, string customerName, List<OrderItemEvent> items)
    {
        OrderId = orderId;
        CustomerName = customerName;
        Items = items;
    }
    public static OrderCreatedEvent Init(string orderId, string customerName, List<OrderItemEvent> items)
    {
        return new OrderCreatedEvent(orderId, customerName, items);
    }
}