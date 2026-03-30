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
        return await _context.Customers.FirstOrDefaultAsync(x => x.Email == email && x.DeletedAt == null);
    }

    public async Task<List<Customer>> FindAsync(Expression<Func<Customer, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.Where(predicate).ToListAsync(cancellationToken);
    }

    public IQueryable<Customer> GetCustomerByPhoneNumberQuery(IQueryable<Customer> query, string searchTerm)
    {
        return query.Where(x => EF.Functions.ILike(
            AppDbContext.FUnaccent(x.PhoneNumber),
            AppDbContext.FUnaccent($"%{searchTerm}%")
        ));
    }
    public IQueryable<Customer> GetCustomerByCustomerNameQuery(IQueryable<Customer> query, string searchTerm)
    {
        return query.Where(x => EF.Functions.ILike(
            AppDbContext.FUnaccent(x.CustomerName),
            AppDbContext.FUnaccent($"%{searchTerm}%")
        ));
    }

    public Task<List<Customer>> ToListAsync(IQueryable<Customer> query, CancellationToken cancellationToken = default)
    {
        return query.ToListAsync(cancellationToken);
    }
}
