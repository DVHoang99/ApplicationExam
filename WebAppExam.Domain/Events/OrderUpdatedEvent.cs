using System;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain.Events;

public class OrderUpdatedEvent : IDomainEvent
{
    public string OrderId { get; private set; }
    public string CustomerName { get; private set; }
    public List<OrderItemEvent> Items { get; private set; }

    private OrderUpdatedEvent(string orderId, string customerName, List<OrderItemEvent> items)
    {
        OrderId = orderId;
        CustomerName = customerName;
        Items = items;
    }

    public static OrderUpdatedEvent Init(string orderId, string customerName, List<OrderItemEvent> items)
    {
        return new OrderUpdatedEvent(orderId, customerName, items);
    }
}
