using System;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Domain.Events;

public class OrderUpdatedEvent : IDomainEvent
{
    public string OrderId { get; init; }
    public string CustomerName { get; init; }
    public string IdempotencyId { get; init; } // Used for ensuring idempotency in event processing and idempotency = key for outbox message
    public List<OrderItemEvent> Items { get; init; }
    public OrderStatus Status { get; init; }

    public OrderUpdatedEvent() { }

    private OrderUpdatedEvent(string orderId, string customerName, List<OrderItemEvent> items, string idempotencyId, OrderStatus status)
    {
        OrderId = orderId;
        CustomerName = customerName;
        Items = items;
        IdempotencyId = idempotencyId;
        Status = status;
    }

    public static OrderUpdatedEvent Init(string orderId, string customerName, List<OrderItemEvent> items, string idempotencyId, OrderStatus status)
    {
        return new OrderUpdatedEvent(orderId, customerName, items, idempotencyId, status);
    }

    public static OrderUpdatedEvent Deserialize(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<OrderUpdatedEvent>(json) ?? throw new InvalidOperationException("Failed to deserialize OrderUpdatedEvent");
    }
}
