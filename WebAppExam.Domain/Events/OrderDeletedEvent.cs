using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain.Events
{
    public class OrderDeletedEvent : IDomainEvent
    {
        public string OrderId { get; private set; }
        public List<OrderItemEvent> Items { get; private set; }

        private OrderDeletedEvent(string orderId, List<OrderItemEvent> items)
        {
            OrderId = orderId;
            Items = items;
        }

        public static OrderDeletedEvent Init(string orderId, List<OrderItemEvent> items)
        {
            return new OrderDeletedEvent(orderId, items);
        }
    }
}