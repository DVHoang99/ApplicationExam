using MediatR;
using WebAppExam.Application.Common;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Ulid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICacheService _cacheService;
    private readonly IInventoryReservationService _inventoryReservationService;

    public DeleteOrderCommandHandler(
        IOrderRepository orderRepository,
        ICacheService cacheService,
        IInventoryReservationService inventoryReservationService)
    {
        _orderRepository = orderRepository;
        _cacheService = cacheService;
        _inventoryReservationService = inventoryReservationService;
    }

    public async Task<Ulid> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);

        if (order == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Order", "Order not found.");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        // 1. Update domain status to Deleted
        order.DeleteOrder();

        // Update the order in the Database
        _orderRepository.Update(order);

        // NOTE: I removed the useless product loop and _productRepository.UpdateRange() here.
        // Deleting an order should NEVER blindly update master product records.

        // 2. Attach Events
        var orderDeletedEvent = new OrderDeletedEvent
        {
            OrderId = order.Id.ToString(),
            Items = order.Details.Select(x => new OrderItemEvent
            {
                ProductId = x.ProductId.ToString(),
                Quantity = -x.Quantity, // Keep your negative quantity logic for the event consumers
                WareHouseId = x.WareHouseId.ToString()
            }).ToList()
        };

        order.AddEventDomain(orderDeletedEvent);

        // Integration event for accounting/finance
        order.AddEventDomain(new OrderCreatedIntegrationEvent(order.Id, -order.TotalAmount, DateTime.UtcNow, -1));

        // 3. REFUND INVENTORY IN REDIS
        // Since the order is deleted, we must release the reserved stock back to the pool
        var itemsToRelease = order.Details.Select(x => new OrderItemDto
        {
            ProductId = x.ProductId,
            WareHouseId = x.WareHouseId.ToString(),
            Quantity = x.Quantity // Pass positive quantity to add it back via Lua INCRBY
        }).ToList();

        if (itemsToRelease.Any())
        {
            await _inventoryReservationService.ReleaseStocksAsync(itemsToRelease);
        }

        // 4. Clear Cache
        await _cacheService.RemoveByPrefixAsync($"order_detail:{request.Id}");

        return order.Id;
    }
}