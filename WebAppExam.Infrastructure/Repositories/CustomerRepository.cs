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

    public IQueryable<Customer> GetCustomerByEmailAsync(string email)
    {
        return Query().Where(x => x.Email == email && x.DeletedAt == null);
    }

    public IQueryable<Customer> GetCustomerByPhoneNumberQuery(IQueryable<Customer> query, string searchTerm)
    {
        return query.Where(x => EF.Functions.ILike(
            x.PhoneNumber,
            $"%{searchTerm}%"
        ));
    }
    public IQueryable<Customer> GetCustomerByCustomerNameQuery(IQueryable<Customer> query, string searchTerm)
    {
        return query.Where(x => EF.Functions.ILike(
            AppDbContext.FUnaccent(x.CustomerName),
            AppDbContext.FUnaccent($"%{searchTerm}%")
        ));
    }

    public IQueryable<Customer> PaginationQuery(IQueryable<Customer> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }
}
