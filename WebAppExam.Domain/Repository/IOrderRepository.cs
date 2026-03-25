using System;
using System.Collections.Generic;
using System.Text;
using WebAppExam.Domain.Common;

namespace WebAppExam.Domain.Repository
{
    internal interface IOrderRepository : IRepository<Order>
    {
        Task<List<Order>> GetByCustomerIdAsync(Ulid customerId);
    }
}
