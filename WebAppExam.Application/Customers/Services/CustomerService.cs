
using System.Reflection.Metadata.Ecma335;
using FluentResults;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Customers.DTOs;
using WebAppExam.Domain;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Customers.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICacheService _cacheService;

    public CustomerService(ICustomerRepository customerRepository, ICacheService cacheService)
    {
        _customerRepository = customerRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<Ulid>> CreateCustomerAync(string customerName, string email, string phone, CancellationToken cancellationToken = default)
    {
        var customerByEmail = await _customerRepository
        .Query()
        .Where(c => c.Email == email && c.DeletedAt == null)
        .AsNoTracking()
        .FirstOrDefaultAsync(cancellationToken);

        if (customerByEmail != null)
        {
            return Result.Fail("Customer already exists");
        }

        var customer = Customer.Create(customerName, email, phone);

        await _customerRepository.AddAsync(customer, cancellationToken);
        return Result.Ok(customer.Id);
    }

    public async Task<Result<List<CustomerDTO>>> GetAllCustomersAsync(string phoneNumber, string customerName, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _customerRepository.Query().Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            query = _customerRepository.GetCustomerByCustomerNameQuery(query, customerName);
        }

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            query = _customerRepository.GetCustomerByPhoneNumberQuery(query, phoneNumber);
        }

        if (pageSize > 0 && pageNumber > 0)
        {
            query = _customerRepository.PaginationQuery(query, pageNumber, pageSize, cancellationToken);
        }

        var customers = await _customerRepository.ToListAsync(query, cancellationToken);

        return customers.Select(x => CustomerDTO.FromResult(x.Id, x.CustomerName, x.Email, x.PhoneNumber)).ToList();
    }

    public async Task<Result<CustomerDTO>> GetCustomerByIdAsync(Ulid id, CancellationToken cancellationToken = default)
    {
        var customer = await _cacheService.GetAsync($"{Constants.CachePrefix.CustomerDetailPrefix}:{id}", async () =>
        {
            var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

            if (customer == null)
            {
                return null;
            }
            return CustomerDTO.FromResult(customer.Id, customer.CustomerName, customer.Email, customer.PhoneNumber);
        }, Constants.CacheDuration.CustomerDetail, cancellationToken);

        if (customer == null)
        {
            return Result.Fail("Customer not found");
        }

        return Result.Ok(customer);
    }

    public async Task<Result<Ulid>> UpdateCustomerAsync(Ulid id, string customerName, string email, string phone, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer == null)
        {
            return Result.Fail("Customer not found");
        }
        customer.Update(customerName, email, phone);

        _customerRepository.Update(customer);

        await _cacheService.RemoveByPrefixAsync($"{Constants.CachePrefix.CustomerDetailPrefix}:{id}");
        return Result.Ok(customer.Id);
    }

    public async Task<Result<Ulid>> DeleteCustomerAsync(Ulid id, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer == null)
        {
            return Result.Fail("Customer not found");
        }

        customer.Delete();

        _customerRepository.Update(customer);

        await _cacheService.RemoveByPrefixAsync($"{Constants.CachePrefix.CustomerDetailPrefix}:{id}");
        return Result.Ok(customer.Id);
    }
}
