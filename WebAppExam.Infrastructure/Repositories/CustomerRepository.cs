using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;

namespace WebAppExam.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context)
    {
    }

    public async Task AddAsync(Customer entity)
    {
        await _context.Customers.AddAsync(entity);
    }

    public async Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        return await _context.Customers.FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<List<Customer>> FindAsync(Expression<Func<Customer, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.Where(predicate).ToListAsync(cancellationToken);
    }
}
