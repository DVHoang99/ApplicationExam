using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Domain.Events
{
    public class OrderCanceledEvent : IDomainEvent
    {
        public string OrderId { get; private set; }
        public OrderStatus Status { get; private set; }
        public List<OrderItemEvent> Items { get; private set; }
        private OrderCanceledEvent(string orderId, OrderStatus status, List<OrderItemEvent> items)
        {
            OrderId = orderId;
            Status = status;
            Items = items;
        }

        public static OrderCanceledEvent Init(string orderId, OrderStatus status, List<OrderItemEvent> items)
        {
            return new OrderCanceledEvent(orderId, status, items);
        }
    }
}