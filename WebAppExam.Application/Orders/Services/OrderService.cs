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
        if (items == null || items.Count == 0) throw new BadRequestException("Items is empty, cannot create order!");

        var customerExists = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customerExists == null) throw new NotFoundException("Customer not found.");

        var groupedItems = GroupOrderItems(items);
        var products = await GetProductsForCreateAsync(groupedItems, cancellationToken);

        // 1. Create Order
        var newOrder = Order.Init(customerId, address, customerName, phoneNumber);
        foreach (var item in groupedItems)
        {
            var product = products[item.ProductId];
            newOrder.AddOrUpdateItem(item.ProductId, product.Price, item.Quantity, Ulid.Parse(item.WareHouseId));
        }

        await _inventoryReservationService.ReserveStocksAsync(groupedItems);

        try
        {
            await _orderRepository.AddAsync(newOrder, cancellationToken);

            var outboxMessages = CreateOutboxMessages(newOrder.Id.ToString(), newOrder.CustomerName, groupedItems, OutboxMessageType.OrderCreated);
            var jobActions = CreateOutboxMessageJobs(outboxMessages, newOrder.Id.ToString(), cancellationToken);

            // Save to DB (Safety Net - Batch)
            await _outboxMessageRepository.AddRangeAsync(outboxMessages, cancellationToken);

            // Enqueue to Hangfire (Performance Optimization: Pass event directly)
            foreach (var job in jobActions) _jobService.Enqueue(job);

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
        var query = _orderRepository.Query()
        .Where(x => x.DeletedAt == null)
        .AsNoTracking();

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
            var order = await _orderRepository.Query()
                .Include(o => o.Details)
                .Where(o => o.Id == id && o.DeletedAt == null)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

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
        if (order == null) throw new NotFoundException("Order not found.");

        var groupedItems = GroupOrderItems(items);
        var products = await GetProductsForUpdateAsync(order, groupedItems, cancellationToken);

        var (itemsToReserve, itemsToRelease, itemsToDelete) = CalculateInventoryDelta(order, groupedItems);

        try
        {
            if (itemsToReserve.Count != 0) await _inventoryReservationService.ReserveStocksAsync(itemsToReserve);
            if (itemsToRelease.Count != 0) await _inventoryReservationService.ReleaseStocksAsync(itemsToRelease);

            var oldTotalAmount = order.TotalAmount;
            order.UpdateOrderGeneralInformation(customerId, customerName, address, phoneNumber);

            var itemUpdated = ApplyItemUpdates(order, groupedItems, itemsToDelete, products);

            _orderRepository.Update(order);
            await UpdateDailyRevenueAsync(order, oldTotalAmount);

            var mappedItemUpdated = itemUpdated.Select(x => new OrderItemDTO(x.ProductId, x.Quantity, x.WareHouseId.ToString()));
            var outboxMessages = CreateOutboxMessages(order.Id.ToString(), order.CustomerName, mappedItemUpdated, OutboxMessageType.OrderUpdated);
            var jobActions = CreateOutboxMessageJobs(outboxMessages, order.Id.ToString(), cancellationToken);

            await _outboxMessageRepository.AddRangeAsync(outboxMessages, cancellationToken);
            foreach (var job in jobActions) _jobService.Enqueue(job);

            await _cacheService.RemoveByPrefixAsync($"{Constants.CachePrefix.OrderDetailPrefix}:{id}");

            return Result.Ok(order.Id);
        }
        catch (Exception ex)
        {
            if (itemsToReserve.Count != 0) await _inventoryReservationService.ReleaseStocksAsync(itemsToReserve);
            if (itemsToRelease.Count != 0) await _inventoryReservationService.ReserveStocksAsync(itemsToRelease);

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

        var mappedItems = order.Details.Select(x => new OrderItemDTO(x.ProductId, -x.Quantity, x.WareHouseId.ToString()));
        var outboxMessages = CreateOutboxMessages(order.Id.ToString(), order.CustomerName, mappedItems, OutboxMessageType.OrderDeleted);

        var jobActions = CreateOutboxMessageJobs(outboxMessages, order.Id.ToString(), cancellationToken);

        await _outboxMessageRepository.AddRangeAsync(outboxMessages, cancellationToken);
        foreach (var job in jobActions)
        {
            _jobService.Enqueue(job);
        }

        var itemsToRelease = order.Details.Select(x => new OrderItemDTO(x.ProductId, x.Quantity, x.WareHouseId.ToString())).ToList();

        if (itemsToRelease.Count != 0)
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

        // order.UpdateOrderStatus(OrderStatus.Updating, "Updating...");
        _orderRepository.Update(order);

        var mappedItems = order.Details.Select(x => new OrderItemDTO(x.ProductId, -x.Quantity, x.WareHouseId.ToString()));
        var outboxMessages = CreateOutboxMessages(order.Id.ToString(), order.CustomerName, mappedItems, OutboxMessageType.OrderCancelled);

        var jobActions = CreateOutboxMessageJobs(outboxMessages, order.Id.ToString(), cancellationToken);

        await _outboxMessageRepository.AddRangeAsync(outboxMessages, cancellationToken);
        foreach (var job in jobActions)
        {
            _jobService.Enqueue(job);
        }

        var itemsToRelease = order.Details.Select(x => new OrderItemDTO(x.ProductId, x.Quantity, x.WareHouseId.ToString())).ToList();

        if (itemsToRelease.Count != 0)
        {
            await _inventoryReservationService.ReleaseStocksAsync(itemsToRelease);
        }

        return order.Id;
    }

    private List<OrderItemDTO> GroupOrderItems(List<OrderItemDTO> items)
    {
        return items
            .GroupBy(x => new { x.ProductId, x.WareHouseId })
            .Select(g => new OrderItemDTO(g.Key.ProductId, g.Sum(x => x.Quantity), g.Key.WareHouseId))
            .OrderBy(x => x.ProductId)
            .ToList();
    }

    private async Task<Dictionary<Ulid, Product>> GetProductsForCreateAsync(List<OrderItemDTO> groupedItems, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetProductByIdsAndWareHouseIdsQuery(
            [.. groupedItems.Select(x => x.ProductId)],
            [.. groupedItems.Select(x => x.WareHouseId)])
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var item in groupedItems)
        {
            if (!products.ContainsKey(item.ProductId))
                throw new NotFoundException($"ProductId {item.ProductId} not found.");
        }

        return products;
    }

    private async Task<Dictionary<Ulid, Product>> GetProductsForUpdateAsync(Order order, List<OrderItemDTO> groupedItems, CancellationToken cancellationToken)
    {
        var allProductIds = groupedItems.Select(x => x.ProductId)
            .Union(order.Details.Select(x => x.ProductId)).ToList();

        var products = await _productRepository
            .GetProductByIdsQuery(allProductIds)
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var req in groupedItems)
        {
            if (!products.ContainsKey(req.ProductId))
                throw new NotFoundException($"ProductId {req.ProductId} not found.");
        }

        return products;
    }

    private (List<OrderItemDTO> ToReserve, List<OrderItemDTO> ToRelease, List<OrderDetail> ToDelete) CalculateInventoryDelta(Order order, List<OrderItemDTO> groupedItems)
    {
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
            var existingItem = order.Details.FirstOrDefault(x => x.ProductId == req.ProductId && x.WareHouseId.ToString() == req.WareHouseId);
            var existingQty = existingItem?.Quantity ?? 0;
            var delta = req.Quantity - existingQty;

            if (delta > 0)
            {
                itemsToReserve.Add(new OrderItemDTO(req.ProductId, delta, req.WareHouseId));
            }
            else if (delta < 0)
            {
                itemsToRelease.Add(new OrderItemDTO(req.ProductId, Math.Abs(delta), req.WareHouseId));
            }
        }

        return (itemsToReserve, itemsToRelease, itemsToDelete);
    }

    private List<OrderDetail> ApplyItemUpdates(Order order, List<OrderItemDTO> groupedItems, List<OrderDetail> itemsToDelete, Dictionary<Ulid, Product> products)
    {
        var itemUpdated = new List<OrderDetail>();

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

        return itemUpdated;
    }

    private async Task UpdateDailyRevenueAsync(Order order, int oldTotalAmount)
    {
        var key = order.CreatedAt.Date.ToString("yyyy-MM-dd");
        var dailyRevenue = await _dailyRepository.GetByKeyAsync(key, CancellationToken.None);
        if (dailyRevenue != null && dailyRevenue.UpdatedAt > order.CreatedAt)
        {
            dailyRevenue.AddDailyRevenue(0, order.TotalAmount - oldTotalAmount);
            _dailyRepository.Update(dailyRevenue);
        }
    }

    private List<OutboxMessage> CreateOutboxMessages(string orderId, string customerName, IEnumerable<OrderItemDTO> items, OutboxMessageType type)
    {
        var outboxMessages = new List<OutboxMessage>();

        foreach (var item in items)
        {
            var itemOutboxId = Ulid.NewUlid();
            var itemProcessedEvent = OrderItemProcessedEvent.Create(
                orderId,
                customerName,
                item.ProductId.ToString(),
                item.Quantity,
                item.WareHouseId.ToString(),
                itemOutboxId.ToString()
            );

            outboxMessages.Add(OutboxMessage.Init(
                itemOutboxId,
                type.Description(),
                JsonSerializer.Serialize(itemProcessedEvent),
                orderId
            ));
        }

        return outboxMessages;
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
