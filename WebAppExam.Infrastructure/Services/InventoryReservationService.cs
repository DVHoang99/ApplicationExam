using System;
using MediatR;
using StackExchange.Redis;
using WebAppExam.Application.Common;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Products.Queries;
using WebAppExam.Application.Products.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Infrastructure.Services;

public class InventoryReservationService : IInventoryReservationService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IMediator _mediator;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryService _inventoryService;

    public InventoryReservationService(
        IConnectionMultiplexer redis,
        IMediator mediator,
        IProductRepository productRepository,
        IInventoryService inventoryService)
    {
        _redis = redis;
        _mediator = mediator;
        _productRepository = productRepository;
        _inventoryService = inventoryService;
    }

    public async Task<bool> ReserveStocksAsync(Ulid customerId, List<OrderItemDto> itemsToReserve)
    {
        var db = _redis.GetDatabase();

        var keys = itemsToReserve
            .Select(x => (RedisKey)$"inventory:stock:{x.WareHouseId}:{x.ProductId}")
            .ToArray();

        var cachedValues = await db.StringGetAsync(keys);

        var missingItems = new List<OrderItemDto>();
        for (int i = 0; i < cachedValues.Length; i++)
        {
            if (cachedValues[i].IsNull)
            {
                missingItems.Add(itemsToReserve[i]);
            }
        }

        if (missingItems.Any())
        {
            var missingProductIds = missingItems.Select(x => x.ProductId).ToList();
            var missingWarehouseIds = missingItems.Select(x => x.WareHouseId.ToString()).ToList();

            var products = await _productRepository.GetProductByIdsAndWareHouseIdsAsync(missingProductIds, missingWarehouseIds);

            var correlationIds = products.Values.Select(x => x.CorrelationId).ToList();

            var inventories = await _inventoryService.GetInventoryDTOsAsync(correlationIds);
            var setCacheTasks = new List<Task>();

            foreach (var missingItem in missingItems)
            {
                var stockInfo = inventories.FirstOrDefault(x => x.ProductId == missingItem.ProductId.ToString() && x.WareHouseId == missingItem.WareHouseId);
                int stockQty = stockInfo != null ? stockInfo.StockQuantity : 0;

                var redisKey = (RedisKey)$"inventory:stock:{missingItem.WareHouseId}:{missingItem.ProductId}";

                setCacheTasks.Add(db.StringSetAsync(redisKey, stockQty, TimeSpan.FromHours(24)));
            }
            await Task.WhenAll(setCacheTasks);
        }

        var values = itemsToReserve.Select(x => (RedisValue)x.Quantity).ToArray();

        var luaScript = @"
            -- 1: Quét dry-run kiểm tra xem tất cả có đủ hàng không
            for i = 1, #KEYS do
                local currentStock = tonumber(redis.call('GET', KEYS[i]) or '0')
                local requestedQty = tonumber(ARGV[i])
                
                if currentStock < requestedQty then
                    return i -- Trả về số thứ tự (index 1-based) của món bị thiếu
                end
            end

            -- 2: Nếu lọt xuống đây nghĩa là 100% đủ hàng -> Gõ búa trừ đồng loạt!
            for i = 1, #KEYS do
                local requestedQty = tonumber(ARGV[i])
                redis.call('DECRBY', KEYS[i], requestedQty)
            end

            return 0 -- thanh cong
        ";

        var result = await db.ScriptEvaluateAsync(luaScript, keys, values);
        int statusCode = (int)result;

        if (statusCode == 0)
        {
            return true;
        }

        var failedIndex = statusCode - 1;
        var failedItem = itemsToReserve[failedIndex];

        throw new FluentValidation.ValidationException(new[] {
            new FluentValidation.Results.ValidationFailure("Inventory", $"Product {failedItem.ProductId} at {failedItem.WareHouseId} not enough.")
        });
    }

    public async Task ReleaseStocksAsync(List<OrderItemDto> itemsToRelease)
    {
        var db = _redis.GetDatabase();

        // Prepare list of Keys and Values (Quantity to refund)
        var keys = itemsToRelease.Select(x => (RedisKey)$"inventory:stock:{x.WareHouseId}:{x.ProductId}").ToArray();
        var values = itemsToRelease.Select(x => (RedisValue)x.Quantity).ToArray();

        // Use Lua Script for safety and speed, loop to ADD back all quantities
        var luaScript = @"
            for i = 1, #KEYS do
                local qtyToRelease = tonumber(ARGV[i])
                -- INCRBY will add the quantity back to the current stock
                redis.call('INCRBY', KEYS[i], qtyToRelease)
            end
        ";

        // Execute Script (Fire and forget, no need to await result here)
        await db.ScriptEvaluateAsync(luaScript, keys, values);
    }
}
