using System;
using FluentResults;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Services;

public interface IOrderService
{
    Task<Result<Ulid>> CreateOrderAsync(Ulid customerId, string address, string phoneNumber, string customerName, List<OrderItemDTO> items, CancellationToken cancellationToken = default);
    Task<Result<List<OrderDTO>>> GetAllOrdersAsync(DateTime? fromDate, DateTime? toDate, string customerName, string phoneNumber, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<Ulid>> UpdateOrderAsync(Ulid id,
        Ulid customerId,
        string customerName,
        string address,
        string phoneNumber,
        List<OrderItemDTO> items,
        CancellationToken cancellationToken = default);
    Task<Result<Ulid>> DeleteOrderAsync(Ulid id, CancellationToken cancellationToken = default);
    Task<Result<Ulid>> CancelOrderAsync(Ulid id, CancellationToken cancellationToken = default);
    Task<Result<OrderDTO>> GetOrderDetailAsync(Ulid id, CancellationToken cancellationToken = default);
}
