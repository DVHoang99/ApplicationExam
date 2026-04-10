using System;
using WebAppExam.Domain;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Events;

namespace WebAppExam.Application.Orders.Events;

public class OrderCreatedEvent : IDomainEvent
{
    public string OrderId { get; init; }
    public string CustomerName { get; init; }
    public string IdempotencyId { get; init; }
    public List<OrderItemEvent> Items { get; init; }

    public OrderCreatedEvent() { }

    private OrderCreatedEvent(string orderId, string customerName, List<OrderItemEvent> items, string idempotencyId)
    {
        OrderId = orderId;
        CustomerName = customerName;
        Items = items;
        IdempotencyId = idempotencyId;
    }
    public static OrderCreatedEvent Init(string orderId, string customerName, List<OrderItemEvent> items, string idempotencyId)
    {
        return new OrderCreatedEvent(orderId, customerName, items, idempotencyId);
    }

    public static OrderCreatedEvent Deserialize(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<OrderCreatedEvent>(json) ?? throw new InvalidOperationException("Failed to deserialize OrderCreatedEvent");
    }
}