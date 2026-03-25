using WebAppExam.Domain.Common;

namespace WebAppExam.Domain.Repository
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<List<Order>> GetByCustomerIdAsync(Ulid customerId);
    }
}
