using WebAppExam.Domain.Common;

namespace WebAppExam.Domain.Repository
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<List<Order>> GetByCustomerIdAsync(Ulid customerId); 
        Task<IEnumerable<Order>> GetByDateAsync(DateTime date);
        IQueryable<Order> GetOrderFromDateToDateAsync(IQueryable<Order> query, DateTime fromDate, DateTime toDate);
        IQueryable<Order> GetOrderByPhoneNumberQuery(IQueryable<Order> query, string searchTerm);
        IQueryable<Order> GetOrderByCustomerNameQuery(IQueryable<Order> query, string searchTerm);
        Task<IEnumerable<Order>> ToListAsync(IQueryable<Order> query, CancellationToken cancellationToken);
    }
}
