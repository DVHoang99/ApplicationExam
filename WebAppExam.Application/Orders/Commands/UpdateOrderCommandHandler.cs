using MediatR;
using WebAppExam.Application.Common;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain;
using WebAppExam.Domain.Enum;
using WebAppExam.Domain.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Ulid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IInventoryReservationService _inventoryReservationService;

    public UpdateOrderCommandHandler(IOrderRepository orderRepository,
    ICacheService cacheService,
    IProductRepository productRepository,
    IInventoryReservationService inventoryReservationService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _cacheService = cacheService;
        _inventoryReservationService = inventoryReservationService;
    }

    public async Task<Ulid> Handle(UpdateOrderCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, ct);
        if (order == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Order", "Order not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        // 1. Group incoming items to avoid duplicates
        var groupedItems = request.Items
            .GroupBy(x => new { x.ProductId, x.WareHouseId })
            .Select(g => new OrderItemDto
            {
                ProductId = g.Key.ProductId,
                WareHouseId = g.Key.WareHouseId,
                Quantity = g.Sum(x => x.Quantity)
            })
            .OrderBy(x => x.ProductId)
            .ToList();

        // 2. Fetch products to validate existence and get latest prices
        var allProductIds = groupedItems.Select(x => x.ProductId)
            .Union(order.Details.Select(x => x.ProductId)).ToList();
        var products = await _productRepository.GetProductByIdsAsync(allProductIds, ct);

        // 3. CALCULATE INVENTORY DELTAS (Reserve vs Release)
        var itemsToReserve = new List<OrderItemDto>();
        var itemsToRelease = new List<OrderItemDto>();

        var requestKeys = groupedItems.Select(x => (x.ProductId, x.WareHouseId)).ToHashSet();

        // 3.1. Find items completely removed from the order
        var itemsToDelete = order.Details
            .Where(dbItem => !requestKeys.Contains((dbItem.ProductId, dbItem.WareHouseId.ToString())))
            .ToList();

        foreach (var del in itemsToDelete)
        {
            itemsToRelease.Add(new OrderItemDto { ProductId = del.ProductId, WareHouseId = del.WareHouseId.ToString(), Quantity = del.Quantity });
        }

        // 3.2. Find items added or quantity increased/decreased
        foreach (var req in groupedItems)
        {
            if (!products.TryGetValue(req.ProductId, out var product))
            {
                var failure = new FluentValidation.Results.ValidationFailure("Product", $"ProductId {req.ProductId} not found.");
                throw new FluentValidation.ValidationException(new[] { failure });
            }

            var existingItem = order.Details.FirstOrDefault(x => x.ProductId == req.ProductId && x.WareHouseId.ToString() == req.WareHouseId);
            var existingQty = existingItem?.Quantity ?? 0;

            var delta = req.Quantity - existingQty;

            if (delta > 0)
            {
                // User increased quantity (or new item) -> Need to reserve more
                itemsToReserve.Add(new OrderItemDto { ProductId = req.ProductId, WareHouseId = req.WareHouseId, Quantity = delta });
            }
            else if (delta < 0)
            {
                // User decreased quantity -> Need to release the freed stock
                itemsToRelease.Add(new OrderItemDto { ProductId = req.ProductId, WareHouseId = req.WareHouseId, Quantity = Math.Abs(delta) });
            }
        }

        // ====================================================================
        // 4. RESERVE NEW STOCK VIA REDIS (Atomic Transaction)
        // ====================================================================
        if (itemsToReserve.Any())
        {
            // If out of stock, this will throw an Exception and abort the update
            await _inventoryReservationService.ReserveStocksAsync(request.CustomerId, itemsToReserve);
        }

        try
        {
            var oldTotalAmount = order.TotalAmount;
            order.UpdateOrderGeneralInformation(request.CustomerId, request.CustomerName, request.Address, request.PhoneNumber);

            var itemUpdated = new List<OrderDetail>();

            // Apply updates to Domain Model
            foreach (var req in groupedItems)
            {
                var product = products[req.ProductId];
                itemUpdated.Add(order.AddOrUpdateItem(req.ProductId, product.Price, req.Quantity, Ulid.Parse(req.WareHouseId)));
            }

            foreach (var del in itemsToDelete)
            {
                var removedItem = order.RemoveItem(del.ProductId, del.WareHouseId.ToString());
                if (removedItem != null) itemUpdated.Add(removedItem);
            }

            order.UpdateOrderStatus(OrderStatus.Draft, "Updating...");

            _orderRepository.Update(order);

            // NOTE: I removed _productRepository.UpdateRange() because updating an order 
            // should NOT blindly update products' master data in the Database. 

            // Attach Events
            var orderUpdateEvent = new OrderUpdatedEvent
            {
                OrderId = order.Id.ToString(),
                CustomerName = request.CustomerName,
                Items = itemUpdated.Select(x => new OrderItemEvent
                {
                    ProductId = x.ProductId.ToString(),
                    Quantity = x.Quantity, // This represents the delta/changed amount
                    WareHouseId = x.WareHouseId.ToString()
                }).ToList()
            };

            order.AddEventDomain(orderUpdateEvent);
            order.AddEventDomain(new OrderCreatedIntegrationEvent(order.Id, order.TotalAmount - oldTotalAmount, DateTime.UtcNow, 0));

            // ====================================================================
            // 5. RELEASE FREED STOCK BACK TO REDIS (Items removed/reduced)
            // ====================================================================
            if (itemsToRelease.Any())
            {
                await _inventoryReservationService.ReleaseStocksAsync(itemsToRelease);
            }

            // Clear Cache
            await _cacheService.RemoveByPrefixAsync($"order_detail:{request.Id}");

            return order.Id;
        }
        catch (Exception ex)
        {
            // ROLLBACK: If DB update fails, we must refund the items we JUST reserved in step 4
            if (itemsToReserve.Any())
            {
                await _inventoryReservationService.ReleaseStocksAsync(itemsToReserve);
            }

            throw new Exception("Error updating order. Reserved inventory has been successfully refunded.", ex);
        }
    }
}
