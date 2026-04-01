using System;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Products.Services;
using WebAppExam.Application.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Ulid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IInventoryService _inventoryService;
    private readonly IJobService _jobService;

    public DeleteProductCommandHandler(IProductRepository productRepository, ICacheService cacheService, IInventoryService inventoryService, IJobService jobService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _inventoryService = inventoryService;
        _jobService = jobService;
    }

    public async Task<Ulid> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, ct);

        if (product == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Product", "Product not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        product.DeletedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        _productRepository.Update(product);

        await _cacheService.RemoveByPrefixAsync($"product_detail:{request.ProductId}");
        _jobService.Enqueue(() => _inventoryService.CallInventoryToDelete(product.Id.ToString(), product.WareHouseId, ct));
        return product.Id;
    }
}
