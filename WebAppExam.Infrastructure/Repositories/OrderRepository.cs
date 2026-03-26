using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;

namespace WebAppExam.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Order>> GetByCustomerIdAsync(Ulid customerId)
    {
        return await _dbSet
            .Where(x => x.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task AddAsync(Order entity)
    {
        await _context.Orders.AddAsync(entity);
    }

    public async Task<Order?> GetByIdAsync(object id)
    {
        return await _context.Orders.FindAsync(id);
    }
}