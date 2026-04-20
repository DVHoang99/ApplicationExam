using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WebAppExam.Application.Common;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Common.Enums;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Infrastructure.Services;

public class InventoryReservationService : IInventoryReservationService
{
    private readonly ICacheService _cacheService;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryService _inventoryService;
    private readonly IDatabase _db;


    // Updated constructor to inject ICacheService
    public InventoryReservationService(
        ICacheService cacheService,
        IProductRepository productRepository,
        IInventoryService inventoryService, IProductService productService)
    {
        _cacheService = cacheService;
        _productRepository = productRepository;
        _inventoryService = inventoryService;
        _db = _cacheService.GetDatabase(RedisDbType.Cache);
    }

    public async Task<bool> ReserveStocksAsync(List<OrderItemDTO> itemsToReserve)
    {

        var keys = itemsToReserve
            .Select(x => (RedisKey)$"inventory:stock:{x.WareHouseId}:{x.ProductId}")
            .ToArray();

        var cachedValues = await _db.StringGetAsync(keys);

        var missingItems = new List<OrderItemDTO>();
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

            var productQuery = _productRepository.GetProductByIdsAndWareHouseIdsQuery(missingProductIds, missingWarehouseIds);

            var products = await productQuery.ToDictionaryAsync(x => x.Id, x => new { x.Id, x.WareHouseId }, CancellationToken.None);

            //var products = await _productRepository.ToDictionaryAsync(productQuery, x => x.Id);

            var productIds = products.Values.Select(x => x.Id.ToString()).ToList();

            //var inventories = await _inventoryService.GetInventoryDTOsAsync(correlationIds);

            var inventoriesResult = await _inventoryService.GetInventoryDTOsByIdsAsync(productIds);
            var inventories = inventoriesResult.IsSuccess ? inventoriesResult.Value : new List<GetBatchInventoryDTO>();
            var setCacheTasks = new List<Task>();

            foreach (var missingItem in missingItems)
            {
                if (inventories != null && inventories.Count > 0)
                {
                    var stockInfo = inventories.FirstOrDefault(x => x.ProductId == missingItem.ProductId.ToString() && x.WareHouseId == missingItem.WareHouseId);
                    int stockQty = stockInfo != null ? stockInfo.StockQuantity : 0;

                    var redisKey = (RedisKey)$"inventory:stock:{missingItem.WareHouseId}:{missingItem.ProductId}";

                    setCacheTasks.Add(_db.StringSetAsync(redisKey, stockQty, TimeSpan.FromHours(24)));
                }
            }
            await Task.WhenAll(setCacheTasks);
        }

        var values = itemsToReserve.Select(x => (RedisValue)x.Quantity).ToArray();

        // Lua Script to ensure atomicity: Check stock and deduct in one go
        // Lua script explanation:
        // 1. Loop through all keys (stock entries) and check if current stock is enough for the requested quantity. If any item fails, return its index (1-based).
        // 2. If all items have enough stock, loop again to deduct the requested quantity from each stock entry.
        // 3. Return 0 to indicate success.

        // Note: Using Lua script ensures that we won't have race conditions where stock might change between the check and the deduction.
        // also, lua using single thread excution in redis.
        
        var luaScript = @"
            -- 1: Scan dry-run to check if all items have enough stock
            for i = 1, #KEYS do
                local currentStock = tonumber(redis.call('GET', KEYS[i]) or '0')
                local requestedQty = tonumber(ARGV[i])
                
                if currentStock < requestedQty then
                    return i -- Return the 1-based index of the failing item
                end
            end

            -- 2: If we reach here, it means 100% enough stock -> Deduct all at once!
            for i = 1, #KEYS do
                local requestedQty = tonumber(ARGV[i])
                redis.call('DECRBY', KEYS[i], requestedQty)
            end

            return 0 -- Success
        ";

        var result = await _db.ScriptEvaluateAsync(luaScript, keys, values);
        int statusCode = (int)result;

        if (statusCode == 0)
        {
            return true;
        }

        var failedIndex = statusCode - 1;
        var failedItem = itemsToReserve[failedIndex];
        var failures = new List<string> { $"Product {failedItem.ProductId} at {failedItem.WareHouseId} not enough." };
        throw new Domain.Exceptions.ValidationException(failures);
    }

    public async Task ReleaseStocksAsync(List<OrderItemDTO> itemsToRelease)
    {
        // Fetch the raw database for the Cache DB using the new service
        var db = _cacheService.GetDatabase(RedisDbType.Cache);

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
        await _db.ScriptEvaluateAsync(luaScript, keys, values);
    }
}