using System;
using FluentResults;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Services;

/// <summary>
/// Defines the contract for order-related operations.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <param name="address">The delivery address.</param>
    /// <param name="phoneNumber">The customer's phone number.</param>
    /// <param name="customerName">The customer's name.</param>
    /// <param name="items">The list of items in the order.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the created order.</returns>
    Task<Result<Ulid>> CreateOrderAsync(Ulid customerId, string address, string phoneNumber, string customerName, List<OrderItemDTO> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of orders based on various filters.
    /// </summary>
    /// <param name="fromDate">The start date for filtering orders.</param>
    /// <param name="toDate">The end date for filtering orders.</param>
    /// <param name="customerName">The name of the customer to filter by.</param>
    /// <param name="phoneNumber">The phone number of the customer to filter by.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of order data transfer objects.</returns>
    Task<Result<List<OrderDTO>>> GetAllOrdersAsync(DateTime? fromDate, DateTime? toDate, string customerName, string phoneNumber, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="id">The unique identifier of the order to update.</param>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <param name="customerName">The customer's name.</param>
    /// <param name="address">The delivery address.</param>
    /// <param name="phoneNumber">The customer's phone number.</param>
    /// <param name="items">The updated list of items in the order.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the updated order.</returns>
    Task<Result<Ulid>> UpdateOrderAsync(Ulid id,
        Ulid customerId,
        string customerName,
        string address,
        string phoneNumber,
        List<OrderItemDTO> items,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the deleted order.</returns>
    Task<Result<Ulid>> DeleteOrderAsync(Ulid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an order.
    /// </summary>
    /// <param name="id">The unique identifier of the order to cancel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the cancelled order.</returns>
    Task<Result<Ulid>> CancelOrderAsync(Ulid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the details of a specific order.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order data transfer object.</returns>
    Task<Result<OrderDTO>> GetOrderDetailAsync(Ulid id, CancellationToken cancellationToken = default);
}
