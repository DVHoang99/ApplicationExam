using System;
using WebAppExam.Domain.Common;

namespace WebAppExam.Domain.Repository;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetCustomerByEmailAsync(string email);
}
