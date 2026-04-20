using System;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Common;

public interface IInventoryReservationService
{
    Task<bool> ReserveStocksAsync(List<OrderItemDTO> itemsToReserve);
    Task ReleaseStocksAsync(List<OrderItemDTO> itemsToRelease);
}
