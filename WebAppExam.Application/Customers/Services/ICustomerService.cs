using System;
using FluentResults;
using WebAppExam.Application.Customers.DTOs;

namespace WebAppExam.Application.Customers.Services;

public interface ICustomerService
{
    Task<Result<Ulid>> CreateCustomerAync(string customerName, string email, string phone, CancellationToken cancellationToken = default);
    Task<Result<List<CustomerDTO>>> GetAllCustomersAsync(string phoneNumber, string customerName, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<CustomerDTO>> GetCustomerByIdAsync(Ulid id, CancellationToken cancellationToken = default);
    Task<Result<Ulid>> UpdateCustomerAsync(Ulid id, string customerName, string email, string phone, CancellationToken cancellationToken);
    Task<Result<Ulid>> DeleteCustomerAsync(Ulid id, CancellationToken cancellationToken);
}
