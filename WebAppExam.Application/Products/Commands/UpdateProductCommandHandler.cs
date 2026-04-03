using System;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Products.Services;
using WebAppExam.Application.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Ulid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IJobService _jobService;
    private readonly IInventoryService _inventoryService;

    public UpdateProductCommandHandler(IProductRepository productRepository, ICacheService cacheService, IJobService jobService, IInventoryService inventoryService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _jobService = jobService;
        _inventoryService = inventoryService;
    }
    public async Task<Ulid> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, ct);

        if (product == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Product", "Product not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        product.UpdateInformation(request.Name, request.Description, request.Price);

        _productRepository.Update(product);

        await _cacheService.RemoveByPrefixAsync($"inventory:stock:{request.WareHouseId}:{request.ProductId}");

        var updateEventId = Guid.NewGuid();

        _jobService.Enqueue(() => _inventoryService.CallInventoryToUpdate(product.Id.ToString(), product.WareHouseId, request.Stock, updateEventId));

        return product.Id;
    }
}
