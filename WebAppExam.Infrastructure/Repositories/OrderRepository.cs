using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain;
using WebAppExam.Domain.Enum;
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

    public new async Task<Order?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.Id == id
                                      && o.DeletedAt == null, cancellationToken);
    }

    public new async Task<List<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Details)
            .Where(o => o.DeletedAt == null)
            .OrderByDescending(o => o.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByDateAsync(DateTime date)
    {
        return await _dbSet
            .Include(o => o.Details)
            .Where(o => o.CreatedAt.Date == date.Date && o.DeletedAt == null && o.Status != OrderStatus.Canceled)
            .ToListAsync();
    }

    public IQueryable<Order> GetOrderFromDateToDateAsync(IQueryable<Order> query, DateTime fromDate, DateTime toDate)
    {
        return query.Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate);
    }

    public IQueryable<Order> GetOrderByPhoneNumberQuery(IQueryable<Order> query, string searchTerm)
    {
        return query.Where(x => EF.Functions.ILike(
            x.PhoneNumber, $"%{searchTerm}%"
            ));
    }

    public IQueryable<Order> GetOrderByCustomerNameQuery(IQueryable<Order> query, string searchTerm)
    {
        return query.Where(x => EF.Functions.ILike(
            AppDbContext.FUnaccent(x.CustomerName),
            AppDbContext.FUnaccent($"%{searchTerm}%")
        ));
    }

    public async Task<IEnumerable<Order>> ToListAsync(IQueryable<Order> query, CancellationToken cancellationToken = default)
    {
        return await query
            .Include(o => o.Details)
            .Where(o => o.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }
    public async Task<Order?> GetOrderByIdAndStatusAsync(Ulid orderId, OrderStatus status, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.DeletedAt == null && o.Status == status, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Details)
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate && o.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }
}