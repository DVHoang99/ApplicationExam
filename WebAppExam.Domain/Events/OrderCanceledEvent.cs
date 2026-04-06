using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Domain.Events
{
    public class OrderCanceledEvent : IDomainEvent
    {
        public string OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItemEvent> Items { get; set; } = new();
    }
}