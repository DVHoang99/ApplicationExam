using System;
using WebAppExam.Domain.Common;

namespace WebAppExam.Domain.Repository;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetCustomerByEmailAsync(string email);
    IQueryable<Customer> GetCustomerByPhoneNumberQuery(IQueryable<Customer> query, string searchTerm);
    IQueryable<Customer> GetCustomerByCustomerNameQuery(IQueryable<Customer> query, string searchTerm);
    IQueryable<Customer> PaginationQuery(IQueryable<Customer> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Customer>> ToListAsync(IQueryable<Customer> query, CancellationToken cancellationToken);
}
