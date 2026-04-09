using System;
using FluentResults;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Services;
using WebAppExam.Domain;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IWareHouseService _wareHouseService;
    private readonly IInventoryService _inventoryService;
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cacheService;
    private readonly IJobService _jobService;

    public ProductService(
        IProductRepository productRepository,
        IWareHouseService wareHouseService,
        IInventoryService inventoryService,
        IUnitOfWork uow,
        ICacheService cacheService,
        IJobService jobService
    )
    {
        _productRepository = productRepository;
        _wareHouseService = wareHouseService;
        _inventoryService = inventoryService;
        _uow = uow;
        _cacheService = cacheService;
        _jobService = jobService;
    }

    public async Task<Result<Ulid>> CreateProductAsync(string name, string? description, int price, string wareHouseId, int stock, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();

        var product = Product.Init(name, description, price, correlationId, wareHouseId);

        var wareHouse = await _wareHouseService.GetWareHouseGrpcAsync(wareHouseId, cancellationToken);

        if (wareHouse == null)
        {
            return Result.Fail("WareHouse not found.");
        }

        await _productRepository.AddAsync(product, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        var inventoryResult = await _inventoryService.CreateInventoryAsync(wareHouse.Id, product.Id.ToString(), stock, correlationId, cancellationToken);

        if (inventoryResult.IsSuccess)
        {
            product.UpdateProductStatus(Domain.Enum.ProductStatus.Active);
            _productRepository.Update(product);
        }

        return Result.Ok(product.Id);
    }

    public async Task<Result<List<ProductDTO>>> GetAllProductsAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _productRepository.Query();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = _productRepository.SearchProductNameQuery(query, searchTerm);
        }

        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var products = await _productRepository.ToListAsync(query, cancellationToken);

        var productIds = products.Select(x => x.Id.ToString()).ToList();

        var inventoriesResult = await _inventoryService.GetInventoryDTOsByIdsAsync(productIds, cancellationToken);

        var inventories = inventoriesResult.IsSuccess ? inventoriesResult.Value : new List<GetBatchInventoryDTO>();
        var inventoriesDictionary = inventories.Count > 0 ? inventories.ToDictionary(x => x.CorrelationId, x => x) : null;

        return Result.Ok(products.Select(x =>
        {
            var inventory = inventoriesDictionary != null && inventoriesDictionary.ContainsKey(x.CorrelationId) ? inventoriesDictionary[x.CorrelationId] : null;
            var wareHouseDTO = inventory != null && inventory.WareHouse != null
            ? WareHouseDTO.Init(inventory.WareHouseId, inventory.WareHouse.Address, inventory.WareHouse.OwnerName, inventory.WareHouse.OwnerEmail, inventory.WareHouse.OwnerPhone)
            : null;

            return ProductDTO.Init
            (x.Id,
            x.Name,
            x.Description,
            x.Price,
            x.WareHouseId,
            inventory == null ? 0 : inventory.StockQuantity,
            wareHouseDTO!);
        }).ToList());
    }

    public async Task<Result<ProductDTO>> GetProductByIdAsync(Ulid id, CancellationToken cancellationToken = default)
    {
        var res = await _productRepository.GetByIdAsync(id, cancellationToken);

        if (res == null)
        {
            return Result.Fail("Product not found.");
        }

        //var inventories = await _inventoryService.GetInventoryDTOsAsync(new List<string> { res.CorrelationId }, cancellationToken);
        var inventoriesResult = await _inventoryService.GetInventoryDTOsByIdsAsync(new List<string> { res.Id.ToString() }, cancellationToken);

        var inventories = inventoriesResult.IsSuccess ? inventoriesResult.Value : new List<GetBatchInventoryDTO>();
        var inventoriesDictionary = inventories.Count > 0 ? inventories.ToDictionary(x => x.CorrelationId, x => x) : null;
        var stock = inventoriesDictionary != null && inventoriesDictionary.ContainsKey(res.CorrelationId) ? inventoriesDictionary[res.CorrelationId].StockQuantity : 0;

        WareHouseDTO? wareHouse = null;

        if (inventoriesDictionary?.TryGetValue(res.CorrelationId, out var inv) == true)
        {
            var wh = inv.WareHouse;

            if (wh != null)
            {
                wareHouse = WareHouseDTO.Init(
                    inv.WareHouseId ?? "",
                    wh.Address ?? "",
                    wh.OwnerName ?? "",
                    wh.OwnerEmail ?? "",
                    wh.OwnerPhone ?? ""
                );
            }
        }

        return Result.Ok(
            ProductDTO.Init(id, res.Name, res.Description, res.Price, res.WareHouseId, stock, wareHouse)
        );
    }

    public async Task<Result<Ulid>> UpdateProductAsync(Ulid id, string name, string? description, int price, string wareHouseId, int stock, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            return Result.Fail("Product not found.");
        }

        product.UpdateInformation(name, description, price);

        _productRepository.Update(product);

        var key = Constants.CachePrefix.InventoriesStock(wareHouseId, id.ToString());

        await _cacheService.RemoveByPrefixAsync(key);

        var updateEventId = Guid.NewGuid();

        _jobService.Enqueue(() => _inventoryService.CallInventoryToUpdate(product.Id.ToString(), product.WareHouseId, stock, updateEventId));

        return product.Id;
    }

    public async Task<Result<Ulid>> DeleteProductAsync(Ulid id, string wareHouseId, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            return Result.Fail("Product not found.");
        }

        product.DeleteProduct();

        _productRepository.Update(product);

        var key = Constants.CachePrefix.InventoriesStock(wareHouseId, id.ToString());

        await _cacheService.RemoveByPrefixAsync(key);
        _jobService.Enqueue(() => _inventoryService.CallInventoryToDelete(product.Id.ToString(), product.WareHouseId, cancellationToken));
        return product.Id;
    }
}
