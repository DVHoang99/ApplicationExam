using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain.Events
{
    public class OrderDeletedEvent : IDomainEvent
    {
        public string OrderId { get; init; }
        public string IdempotencyId { get; init; }
        public List<OrderItemEvent> Items { get; init; }

        public OrderDeletedEvent() { }

        private OrderDeletedEvent(string orderId, List<OrderItemEvent> items, string idempotencyId)
        {
            OrderId = orderId;
            Items = items;
            IdempotencyId = idempotencyId;
        }

        public static OrderDeletedEvent Init(string orderId, List<OrderItemEvent> items, string idempotencyId)
        {
            return new OrderDeletedEvent(orderId, items, idempotencyId);
        }

        public static OrderDeletedEvent Deserialize(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<OrderDeletedEvent>(json) ?? throw new InvalidOperationException("Failed to deserialize OrderDeletedEvent");
        }
    }
}