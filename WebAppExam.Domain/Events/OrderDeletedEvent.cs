using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain.Events
{
    public class OrderDeletedEvent : IDomainEvent
    {
        public string OrderId { get; set; }
        public List<OrderItemEvent> Items { get; set; } = new();
    }
}