using System;
using FluentResults;
using WebAppExam.Application.Customers.DTOs;

namespace WebAppExam.Application.Customers.Services;

/// <summary>
/// Defines the contract for customer-related operations.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="customerName">The name of the customer.</param>
    /// <param name="email">The customer's email address.</param>
    /// <param name="phone">The customer's phone number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the created customer.</returns>
    Task<Result<Ulid>> CreateCustomerAync(string customerName, string email, string phone, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of customers based on filters.
    /// </summary>
    /// <param name="phoneNumber">The phone number to filter by.</param>
    /// <param name="customerName">The customer name to filter by.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of customer data transfer objects.</returns>
    Task<Result<List<CustomerDTO>>> GetAllCustomersAsync(string phoneNumber, string customerName, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer data transfer object.</returns>
    Task<Result<CustomerDTO>> GetCustomerByIdAsync(Ulid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing customer's information.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="customerName">The updated name of the customer.</param>
    /// <param name="email">The updated email address.</param>
    /// <param name="phone">The updated phone number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the updated customer.</returns>
    Task<Result<Ulid>> UpdateCustomerAsync(Ulid id, string customerName, string email, string phone, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the deleted customer.</returns>
    Task<Result<Ulid>> DeleteCustomerAsync(Ulid id, CancellationToken cancellationToken);
}
