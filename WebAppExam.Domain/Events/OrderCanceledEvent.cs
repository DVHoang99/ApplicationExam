using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Domain.Events
{
    public class OrderCanceledEvent : IDomainEvent
    {
        public string OrderId { get; init; }
        public OrderStatus Status { get; init; }
        public string IdempotencyId { get; init; }
        public List<OrderItemEvent> Items { get; init; }

        public OrderCanceledEvent() { }

        private OrderCanceledEvent(string orderId, OrderStatus status, List<OrderItemEvent> items, string idempotencyId)
        {
            OrderId = orderId;
            Status = status;
            Items = items;
            IdempotencyId = idempotencyId;
        }

        public static OrderCanceledEvent Init(string orderId, OrderStatus status, List<OrderItemEvent> items, string idempotencyId)
        {
            return new OrderCanceledEvent(orderId, status, items, idempotencyId);
        }

        public static OrderCanceledEvent Deserialize(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<OrderCanceledEvent>(json) ?? throw new InvalidOperationException("Failed to deserialize OrderCanceledEvent");
        }
    }
}