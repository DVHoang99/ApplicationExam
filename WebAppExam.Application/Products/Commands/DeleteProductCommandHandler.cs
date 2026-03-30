using System;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Ulid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;



    public DeleteProductCommandHandler(IProductRepository productRepository, ICacheService cacheService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
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

        foreach (var item in product.Inventories)
        {
            product.DeleteInventory(item.Id);
        }

        _productRepository.Update(product);

        await _cacheService.RemoveByPrefixAsync($"product_detail:{request.ProductId}");

        return product.Id;
    }
}
