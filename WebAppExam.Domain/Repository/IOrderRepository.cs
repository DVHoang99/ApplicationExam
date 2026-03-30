using WebAppExam.Domain.Common;

namespace WebAppExam.Domain.Repository
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<List<Order>> GetByCustomerIdAsync(Ulid customerId); 
        Task<IEnumerable<Order>> GetByDateAsync(DateTime date);
        void GetByDateAsync(DateTime date);
    }
}
