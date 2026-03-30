using System;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Ulid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;

    public UpdateProductCommandHandler(IProductRepository productRepository, ICacheService cacheService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
    }
    public async Task<Ulid> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, ct);

        if (product == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Product", "Product not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;

        foreach (var inventory in request.Inventories)
        {
            product.AddOrUpdateInventory(inventory.Id, inventory.Stock, product.Id, inventory.Name);
        }

        _productRepository.Update(product);

        await _cacheService.RemoveByPrefixAsync($"product_detail:{request.ProductId}");

        return product.Id;
    }
}
