using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.Common;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Enum;
using WebAppExam.Domain.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Ulid>
{
    public readonly IOrderRepository _orderRepository;
    public readonly IInventoryReservationService _inventoryReservationService;


    public CancelOrderCommandHandler(IOrderRepository orderRepository, IInventoryReservationService inventoryReservationService)
    {
        _orderRepository = orderRepository;
        _inventoryReservationService = inventoryReservationService;
    }

    public async Task<Ulid> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);

        if (order == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Order", "Order not found.");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        var statusPrevious = order.Status;

        order.UpdateOrderStatus(OrderStatus.Updating, "Updating...");
        _orderRepository.Update(order);

        var orderCanceledEvent = new OrderCanceledEvent
        {
            OrderId = order.Id.ToString(),
            Status = statusPrevious,
            Items = order.Details.Select(x => new OrderItemEvent
            {
                ProductId = x.ProductId.ToString(),
                Quantity = -x.Quantity,
                WareHouseId = x.WareHouseId.ToString()
            }).ToList()
        };

        order.AddEventDomain(orderCanceledEvent);

        order.AddEventDomain(new OrderCreatedIntegrationEvent(order.Id, -order.TotalAmount, DateTime.UtcNow, -1));

        var itemsToRelease = order.Details.Select(x => new OrderItemDto
        {
            ProductId = x.ProductId,
            WareHouseId = x.WareHouseId.ToString(),
            Quantity = x.Quantity
        }).ToList();

        if (itemsToRelease.Any())
        {
            await _inventoryReservationService.ReleaseStocksAsync(itemsToRelease);
        }


        return order.Id;

    }
}
