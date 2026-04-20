using System.Text.Json;
using System.Linq.Expressions;
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
using WebAppExam.Domain.Exceptions;
using WebAppExam.Domain.Repository;
using WebAppExam.Application.Orders.Commands;

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
            throw new BadRequestException("Items is empty, cannot create order!");
        }

        var customerExists = await _customerRepository.GetByIdAsync(customerId, cancellationToken);

        if (customerExists == null)
        {
            throw new NotFoundException("Customer not found.");
        }

        var groupedItems = items
            .GroupBy(x => new { x.ProductId, x.WareHouseId })
            .Select(g => new OrderItemDTO(g.Key.ProductId, g.Sum(x => x.Quantity), g.Key.WareHouseId))
            .OrderBy(x => x.ProductId)
            .ToList();

        var products = await _productRepository.GetProductByIdsAndWareHouseIdsQuery(
            [.. groupedItems.Select(x => x.ProductId)],
            [.. groupedItems.Select(x => x.WareHouseId)])
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        // 1. Create Order
        var newOrder = Order.Init(customerId, address, customerName, phoneNumber);

        foreach (var item in groupedItems)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                throw new NotFoundException($"ProductId {item.ProductId} not found.");
            }

            newOrder.AddOrUpdateItem(item.ProductId, product.Price, item.Quantity, Ulid.Parse(item.WareHouseId));
        }

        await _inventoryReservationService.ReserveStocksAsync(groupedItems);

        try
        {
            await _orderRepository.AddAsync(newOrder, cancellationToken);

            var outboxMessages = new List<OutboxMessage>();

            // 2. Create one outbox message for each item
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

                var itemOutbox = OutboxMessage.Init(
                    itemOutboxId,
                    OutboxMessageType.OrderCreated.Description(),
                    JsonSerializer.Serialize(itemProcessedEvent),
                    newOrder.Id.ToString()
                );

                outboxMessages.Add(itemOutbox);
            }

            var jobActions = CreateOutboxMessageJobs(outboxMessages, newOrder.Id.ToString(), cancellationToken);

            // Save to DB (Safety Net - Batch)
            await _outboxMessageRepository.AddRangeAsync(outboxMessages, cancellationToken);

            // Enqueue to Hangfire (Performance Optimization: Pass event directly)
            foreach (var job in jobActions)
            {
                _jobService.Enqueue(job);
            }

            return Result.Ok(newOrder.Id);
        }
        catch (Exception ex)
        {
            await _inventoryReservationService.ReleaseStocksAsync(groupedItems);
            throw new BadRequestException($"Error saving order. Inventory has been reverted: {ex.Message}");
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
            order.CreatedAt,
            [.. order.Details]))
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
                [.. order.Details]);
            return res;
        }, Constants.CacheDuration.OrderDetail, cancellationToken);

        if (orderDTO == null)
        {
            throw new NotFoundException("Order not found.");
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
            throw new NotFoundException("Order not found.");
        }

        var groupedItems = items
            .GroupBy(x => new { x.ProductId, x.WareHouseId })
            .Select(g => new OrderItemDTO(g.Key.ProductId, g.Sum(x => x.Quantity), g.Key.WareHouseId))
            .OrderBy(x => x.ProductId)
            .ToList();

        var allProductIds = groupedItems.Select(x => x.ProductId)
            .Union(order.Details.Select(x => x.ProductId)).ToList();

        var products = await _productRepository
            .GetProductByIdsQuery(allProductIds)
            .AsNoTracking()
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
                throw new NotFoundException($"ProductId {req.ProductId} not found.");
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

        try
        {
            if (itemsToReserve.Any())
            {
                await _inventoryReservationService.ReserveStocksAsync(itemsToReserve);
            }

            if (itemsToRelease.Any())
            {
                await _inventoryReservationService.ReleaseStocksAsync(itemsToRelease);
            }

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

            _orderRepository.Update(order);

            var key = order.CreatedAt.Date.ToString("yyyy-MM-dd");
            var dailyRevenue = await _dailyRepository.GetByKeyAsync(key, CancellationToken.None);
            if (dailyRevenue != null && dailyRevenue.UpdatedAt > order.CreatedAt)
            {
                dailyRevenue.AddDailyRevenue(0, order.TotalAmount - oldTotalAmount);
                _dailyRepository.Update(dailyRevenue);
            }

            var outboxMessages = new List<OutboxMessage>();

            // 2. Create one outbox message for each item (Updated or Added)
            foreach (var item in itemUpdated)
            {
                var itemOutboxId = Ulid.NewUlid();
                var itemProcessedEvent = OrderItemProcessedEvent.Create(
                    order.Id.ToString(),
                    order.CustomerName,
                    item.ProductId.ToString(),
                    item.Quantity,
                    item.WareHouseId.ToString(),
                    itemOutboxId.ToString()
                );

                var itemOutbox = OutboxMessage.Init(
                    itemOutboxId,
                    OutboxMessageType.OrderUpdated.Description(),
                    JsonSerializer.Serialize(itemProcessedEvent),
                    order.Id.ToString()
                );

                outboxMessages.Add(itemOutbox);
            }

            var jobActions = CreateOutboxMessageJobs(outboxMessages, order.Id.ToString(), cancellationToken);

            // Save to DB (Safety Net - Batch)
            await _outboxMessageRepository.AddRangeAsync(outboxMessages, cancellationToken);

            // Enqueue to Hangfire (Performance Optimization: Pass event directly)
            foreach (var job in jobActions)
            {
                _jobService.Enqueue(job);
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

            if (itemsToRelease.Any())
            {
                await _inventoryReservationService.ReserveStocksAsync(itemsToRelease);
            }

            throw new BadRequestException($"Error updating order. Reserved inventory has been successfully refunded: {ex.Message}");
        }
    }

    public async Task<Result<Ulid>> DeleteOrderAsync(Ulid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException("Order not found.");
        }

        order.DeleteOrder();

        _orderRepository.Update(order);

        var outboxMessages = new List<OutboxMessage>();

        foreach (var detail in order.Details)
        {
            var itemOutboxId = Ulid.NewUlid();
            var itemProcessedEvent = OrderItemProcessedEvent.Create(
                order.Id.ToString(),
                order.CustomerName,
                detail.ProductId.ToString(),
                -detail.Quantity,
                detail.WareHouseId.ToString(),
                itemOutboxId.ToString()
            );

            var itemOutbox = OutboxMessage.Init(
                itemOutboxId,
                OutboxMessageType.OrderDeleted.Description(),
                JsonSerializer.Serialize(itemProcessedEvent),
                order.Id.ToString()
            );

            outboxMessages.Add(itemOutbox);
        }

        var jobActions = CreateOutboxMessageJobs(outboxMessages, order.Id.ToString(), cancellationToken);

        await _outboxMessageRepository.AddRangeAsync(outboxMessages, cancellationToken);
        foreach (var job in jobActions)
        {
            _jobService.Enqueue(job);
        }

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
            throw new NotFoundException("Order not found.");
        }

        var statusPrevious = order.Status;

        order.UpdateOrderStatus(OrderStatus.Updating, "Updating...");
        _orderRepository.Update(order);

        var outboxMessages = new List<OutboxMessage>();

        foreach (var detail in order.Details)
        {
            var itemOutboxId = Ulid.NewUlid();
            var itemProcessedEvent = OrderItemProcessedEvent.Create(
                order.Id.ToString(),
                order.CustomerName,
                detail.ProductId.ToString(),
                -detail.Quantity, // Negative value signifies cancellation/revert
                detail.WareHouseId.ToString(),
                itemOutboxId.ToString()
            );

            var itemOutbox = OutboxMessage.Init(
                itemOutboxId,
                OutboxMessageType.OrderCancelled.Description(),
                JsonSerializer.Serialize(itemProcessedEvent),
                 order.Id.ToString()
            );

            outboxMessages.Add(itemOutbox);
        }

        var jobActions = CreateOutboxMessageJobs(outboxMessages, order.Id.ToString(), cancellationToken);

        await _outboxMessageRepository.AddRangeAsync(outboxMessages, cancellationToken);
        foreach (var job in jobActions)
        {
            _jobService.Enqueue(job);
        }

        var itemsToRelease = order.Details.Select(x => new OrderItemDTO(x.ProductId, x.Quantity, x.WareHouseId.ToString())).ToList();

        if (itemsToRelease.Any())
        {
            await _inventoryReservationService.ReleaseStocksAsync(itemsToRelease);
        }

        return order.Id;
    }

    private List<Expression<Func<IOutboxService, Task>>> CreateOutboxMessageJobs(List<OutboxMessage> outboxMessages, string orderId, CancellationToken cancellationToken)
    {
        var jobActions = new List<Expression<Func<IOutboxService, Task>>>();

        foreach (var outbox in outboxMessages)
        {
            var itemProcessedEvent = JsonSerializer.Deserialize<OrderItemProcessedEvent>(outbox.Content);

            if (itemProcessedEvent != null)
            {
                jobActions.Add(s => s.PublishMessageAsync(outbox.Id, orderId, itemProcessedEvent, CancellationToken.None));
            }
        }

        return jobActions;
    }
}
