using System;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Common;

public interface IInventoryReservationService
{
    Task<bool> ReserveStocksAsync(Ulid customerId, List<OrderItemDto> itemsToReserve);
    Task ReleaseStocksAsync(List<OrderItemDto> itemsToRelease);
}
