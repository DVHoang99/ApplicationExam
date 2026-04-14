using System;
using WebAppExam.Application.Products.Services;
using WebAppExam.Application.Services;
using WebAppExam.Domain.Repository;
using Microsoft.Extensions.Logging;

namespace WebAppExam.Infrastructure.Services;

public class InventoryReconciliationJob : IInventoryReconciliationJob
{
    private readonly IProductRepository _productRepository;
    private readonly IInventoryService _inventoryService;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<InventoryReconciliationJob> _logger;

    public InventoryReconciliationJob(IProductRepository productRepository, IInventoryService inventoryService, IUnitOfWork uow, ILogger<InventoryReconciliationJob> logger)
    {
        _productRepository = productRepository;
        _inventoryService = inventoryService;
        _uow = uow;
        _logger = logger;
    }

    public async Task ReconcilePendingProductsAsync()
    {
        var pendingProducts = await _productRepository.GetProductsNotSync();

        if (!pendingProducts.Any()) return;

        _logger.LogInformation("Found {Count} pending products to reconcile.", pendingProducts.Count);

        foreach (var product in pendingProducts)
        {
            var result = await _inventoryService.CreateInventoryGrpcAsync(
                    product.Id.ToString(),
                    product.WareHouseId,
                    10,
                    product.CorrelationId);

            if (result != null)
            {
                product.UpdateProductStatus(Domain.Enum.ProductStatus.Active);
                _productRepository.Update(product);
                _logger.LogInformation("Product {Id} reconciled successfully.", product.Id);
            }
        }
        await _uow.SaveChangesAsync();
    }

}
