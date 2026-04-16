using System.Text.Json;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Application.Common;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Application.OutboxMessages;
using WebAppExam.Application.Services;
using WebAppExam.Domain;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;
using WebAppExam.Domain.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Services;

public class OrderService : IOrderService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryReservationService _inventoryReservationService;
    private readonly ICacheService _cacheService;
    private readonly IDailyRevenueRepository _dailyRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IHangfireJobService _jobService;

    public OrderService(
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IInventoryReservationService inventoryReservationService,
        ICacheService cacheService,
        IDailyRevenueRepository dailyRepository,
        IOutboxMessageRepository outboxMessageRepository,
        IHangfireJobService jobService
        )
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _inventoryReservationService = inventoryReservationService;
        _cacheService = cacheService;
        _dailyRepository = dailyRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _jobService = jobService;
    }

    public async Task<Result<Ulid>> CreateOrderAsync(Ulid customerId, string address, string phoneNumber, string customerName, List<OrderItemDTO> items, CancellationToken cancellationToken = default)
    {
        if (items == null || items.Count == 0)
        {
            return Result.Fail("Cart is empty, cannot create order!");
        }

        var customerExists = await _customerRepository.GetByIdAsync(customerId, cancellationToken);

        if (customerExists == null)
        {
            return Result.Fail("Customer not found.");
        }

        var groupedItems = items
            .GroupBy(x => new { x.ProductId, x.WareHouseId })
            .Select(g => new OrderItemDTO(g.Key.ProductId, g.Sum(x => x.Quantity), g.Key.WareHouseId))
            .OrderBy(x => x.ProductId)
            .ToList();

        var products = await _productRepository.GetProductByIdsAndWareHouseIdsQuery(
            groupedItems.Select(x => x.ProductId).ToList(),
            groupedItems.Select(x => x.WareHouseId).ToList())
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        // 1. Create Order
        var newOrder = Order.Init(customerId, address, customerName, phoneNumber);

        foreach (var item in groupedItems)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                return Result.Fail($"ProductId {item.ProductId} not found.");
            }

            newOrder.AddOrUpdateItem(item.ProductId, product.Price, item.Quantity, Ulid.Parse(item.WareHouseId));
        }

        await _inventoryReservationService.ReserveStocksAsync(customerId, groupedItems);

        try
        {
            await _orderRepository.AddAsync(newOrder, cancellationToken);

            // 2. Create one outbox message for each item and enqueue immediate publication
            foreach (var item in groupedItems)
            {
                var itemOutboxId = Ulid.NewUlid();
                var itemProcessedEvent = OrderItemProcessedEvent.Create(
                    newOrder.Id.ToString(),
                    newOrder.CustomerName,
                    item.ProductId.ToString(),
                    item.Quantity,
                    item.WareHouseId.ToString(),
                    itemOutboxId.ToString()
                );

                var kafkaKey = $"{Constants.KafkaPrefix.OrderCreatedPrefix}:{newOrder.Id}:{item.ProductId}";

                var itemOutbox = OutboxMessage.Init(
                    itemOutboxId,
                    nameof(OrderItemProcessedEvent),
                    JsonSerializer.Serialize(itemProcessedEvent),
                    kafkaKey
                );

                // Save to DB (Safety Net)
                await _outboxMessageRepository.AddAsync(itemOutbox, cancellationToken);
                
                // Enqueue to Hangfire (Performance Optimization: Pass event directly)
                // This fulfills: "just publish event dont get outbox, it just send messgae"
                _jobService.Enqueue<IOutboxService>(s => s.PublishMessageAsync(itemOutboxId, kafkaKey, itemProcessedEvent, CancellationToken.None));
            }

            return Result.Ok(newOrder.Id);
        }
        catch (Exception ex)
        {
            await _inventoryReservationService.ReleaseStocksAsync(groupedItems);
            return Result.Fail($"Error saving order. Inventory has been reverted: {ex}");
        }
    }

    public async Task<Result<List<OrderDTO>>> GetAllOrdersAsync(
        DateTime? fromDate,
        DateTime? toDate,
        string customerName,
        string phoneNumber,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _orderRepository.Query();

        if (fromDate != null && toDate != null)
        {
            query = _orderRepository.GetOrderFromDateToDateAsync(query, fromDate.Value, toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            query = _orderRepository.GetOrderByCustomerNameQuery(query, customerName);
        }

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            query = _orderRepository.GetOrderByPhoneNumberQuery(query, phoneNumber);
        }

        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var maps = await _orderRepository.ToListAsync(query, cancellationToken);

        return maps.Select(order => OrderDTO.Init(
            order.Id,
            order.CustomerId,
            order.Status,
            order.TotalAmount,
            order.Address,
            order.CustomerName,
            order.PhoneNumber,
            order.CreatedAt, order.Details.ToList()))
            .ToList();
    }

    public async Task<Result<OrderDTO>> GetOrderDetailAsync(Ulid id, CancellationToken cancellationToken = default)
    {
        var key = $"{Constants.CachePrefix.OrderDetailPrefix}:{id}";
        var orderDTO = await _cacheService.GetAsync<OrderDTO?>(key, async () =>
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

            if (order == null)
            {
                return null;
            }

            var res = OrderDTO.Init(
                order.Id,
                order.CustomerId,
                order.Status,
                order.TotalAmount,
                order.Address,
                order.CustomerName,
                order.PhoneNumber,
                order.CreatedAt,
                order.Details.ToList());
            return res;
        }, Constants.CacheDuration.OrderDetail, cancellationToken);

        if (orderDTO == null)
        {
            return Result.Fail("Order not found.");
        }

        return Result.Ok(orderDTO);
    }

    public async Task<Result<Ulid>> UpdateOrderAsync(Ulid id,
        Ulid customerId,
        string customerName,
        string address,
        string phoneNumber,
        List<OrderItemDTO> items,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            return Result.Fail("Order not found.");
        }

        var groupedItems = items
            .GroupBy(x => new { x.ProductId, x.WareHouseId })
            .Select(g => new OrderItemDTO(g.Key.ProductId, g.Sum(x => x.Quantity), g.Key.WareHouseId))
            .OrderBy(x => x.ProductId)
            .ToList();

        var allProductIds = groupedItems.Select(x => x.ProductId)
            .Union(order.Details.Select(x => x.ProductId)).ToList();
        
        var products = await _productRepository.GetProductByIdsQuery(allProductIds)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var itemsToReserve = new List<OrderItemDTO>();
        var itemsToRelease = new List<OrderItemDTO>();

        var requestKeys = groupedItems.Select(x => (x.ProductId, x.WareHouseId)).ToHashSet();

        var itemsToDelete = order.Details
            .Where(dbItem => !requestKeys.Contains((dbItem.ProductId, dbItem.WareHouseId.ToString())))
            .ToList();

        foreach (var del in itemsToDelete)
        {
            itemsToRelease.Add(new OrderItemDTO(del.ProductId, del.Quantity, del.WareHouseId.ToString()));
        }

        foreach (var req in groupedItems)
        {
            if (!products.TryGetValue(req.ProductId, out var product))
            {
                return Result.Fail($"ProductId {req.ProductId} not found.");
            }

            var existingItem = order.Details.FirstOrDefault(x => x.ProductId == req.ProductId && x.WareHouseId.ToString() == req.WareHouseId);
            var existingQty = existingItem?.Quantity ?? 0;

            var delta = req.Quantity - existingQty;

            if (delta > 0)
            {
                // User increased quantity (or new item) -> Need to reserve more
                itemsToReserve.Add(new OrderItemDTO(req.ProductId, delta, req.WareHouseId));
            }
            else if (delta < 0)
            {
                // User decreased quantity -> Need to release the freed stock
                itemsToRelease.Add(new OrderItemDTO(req.ProductId, Math.Abs(delta), req.WareHouseId));
            }
        }

        if (itemsToReserve.Any())
        {
            await _inventoryReservationService.ReserveStocksAsync(customerId, itemsToReserve);
        }

        try
        {
            var oldTotalAmount = order.TotalAmount;
            order.UpdateOrderGeneralInformation(customerId, customerName, address, phoneNumber);

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

            var previousStatus = order.Status;

            order.UpdateOrderStatus(OrderStatus.Updating, "Updating...");

            _orderRepository.Update(order);

            var key = order.CreatedAt.Date.ToString("yyyy-MM-dd");
            var dailyRevenue = await _dailyRepository.GetByKeyAsync(key, CancellationToken.None);
            if (dailyRevenue != null && dailyRevenue.UpdatedAt > order.CreatedAt)
            {
                dailyRevenue.AddDailyRevenue(0, order.TotalAmount - oldTotalAmount);
                _dailyRepository.Update(dailyRevenue);
            }

            var itemUpdatedEvent = itemUpdated.Select(x => OrderItemEvent.Init(x.ProductId.ToString(), x.Quantity, x.WareHouseId.ToString())).ToList();

            var outboxMessageId = Ulid.NewUlid();

            // Attach Events
            var orderUpdateEvent = OrderUpdatedEvent.Init(order.Id.ToString(), order.CustomerName, itemUpdatedEvent, outboxMessageId.ToString(), previousStatus);

            var contentMessage = JsonSerializer.Serialize(orderUpdateEvent);

            var outboxMessage = OutboxMessage.Init(
                outboxMessageId,
                nameof(OrderUpdatedEvent),
                contentMessage,
                $"{Constants.KafkaPrefix.OrderUpdatePrefix}:{orderUpdateEvent.OrderId}"
            );

            await _outboxMessageRepository.AddAsync(outboxMessage, cancellationToken);

            order.AddDomainEvent(orderUpdateEvent);
            order.AddDomainEvent(new OrderCreatedIntegrationEvent(order.Id, order.TotalAmount - oldTotalAmount, DateTime.UtcNow, 0));

            if (itemsToRelease.Any())
            {
                await _inventoryReservationService.ReleaseStocksAsync(itemsToRelease);
            }

            var cacheKey = $"{Constants.CachePrefix.OrderDetailPrefix}:{id}";
            // Clear Cache
            await _cacheService.RemoveByPrefixAsync(cacheKey);

            return Result.Ok(order.Id);
        }
        catch (Exception ex)
        {
            if (itemsToReserve.Any())
            {
                await _inventoryReservationService.ReleaseStocksAsync(itemsToReserve);
            }

            return Result.Fail($"Error updating order. Reserved inventory has been successfully refunded: {ex.Message}");
        }
    }

    public async Task<Result<Ulid>> DeleteOrderAsync(Ulid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order == null)
        {
            return Result.Fail("Order not found.");
        }

        order.DeleteOrder();

        _orderRepository.Update(order);

        var itemDeletedEvent = order.Details.Select(x => OrderItemEvent.Init(x.ProductId.ToString(), -x.Quantity, x.WareHouseId.ToString())).ToList();

        var outboxMessageId = Ulid.NewUlid();

        var orderDeletedEvent = OrderDeletedEvent.Init(order.Id.ToString(), itemDeletedEvent, outboxMessageId.ToString());

        var contentMessage = JsonSerializer.Serialize(orderDeletedEvent);

        var outboxMessage = OutboxMessage.Init(
            outboxMessageId,
            nameof(OrderDeletedEvent),
            contentMessage,
            $"{Constants.KafkaPrefix.OrderDeletedPrefix}:{orderDeletedEvent.OrderId}"
        );

        await _outboxMessageRepository.AddAsync(outboxMessage, cancellationToken);

        order.AddDomainEvent(orderDeletedEvent);

        order.AddDomainEvent(new OrderCreatedIntegrationEvent(order.Id, -order.TotalAmount, DateTime.UtcNow, -1));

        var itemsToRelease = order.Details.Select(x => new OrderItemDTO(x.ProductId, x.Quantity, x.WareHouseId.ToString())).ToList();

        if (itemsToRelease.Any())
        {
            await _inventoryReservationService.ReleaseStocksAsync(itemsToRelease);
        }

        itemsToRelease.ForEach(x => order.RemoveItem(x.ProductId, x.WareHouseId));

        var cacheKey = $"{Constants.CachePrefix.OrderDetailPrefix}:{id}";
        await _cacheService.RemoveByPrefixAsync(cacheKey);

        return Result.Ok(order.Id);
    }

    public async Task<Result<Ulid>> CancelOrderAsync(Ulid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order == null || order.Status == OrderStatus.Canceled)
        {
            return Result.Fail("Order not found.");
        }

        var statusPrevious = order.Status;

        order.UpdateOrderStatus(OrderStatus.Updating, "Updating...");
        _orderRepository.Update(order);

        var outboxMessageId = Ulid.NewUlid();

        var orderCanceledEvent = OrderCanceledEvent.Init
        (order.Id.ToString(),
        statusPrevious,
        order.Details.Select(o => OrderItemEvent.Init(o.ProductId.ToString(), -o.Quantity, o.WareHouseId.ToString())).ToList(),
        outboxMessageId.ToString());

        var contentMessage = JsonSerializer.Serialize(orderCanceledEvent);

        var outboxMessage = OutboxMessage.Init(
            outboxMessageId,
            nameof(OrderCanceledEvent),
            contentMessage,
            $"{Constants.KafkaPrefix.OrderCanceledPrefix}:{orderCanceledEvent.OrderId}"
        );

        await _outboxMessageRepository.AddAsync(outboxMessage, cancellationToken);

        order.AddDomainEvent(orderCanceledEvent);

        order.AddDomainEvent(new OrderCreatedIntegrationEvent(order.Id, -order.TotalAmount, DateTime.UtcNow, -1));

        var itemsToRelease = order.Details.Select(x => new OrderItemDTO(x.ProductId, x.Quantity, x.WareHouseId.ToString())).ToList();

        if (itemsToRelease.Any())
        {
            await _inventoryReservationService.ReleaseStocksAsync(itemsToRelease);
        }

        return order.Id;
    }
}
