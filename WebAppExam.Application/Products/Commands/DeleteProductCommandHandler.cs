using System;
using MediatR;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Ulid>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Ulid> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, ct);

        if (product == null)
            throw new Exception("Product not found");

        product.DeletedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        foreach (var item in product.Inventories)
        {
            product.DeleteInventory(item.Id);
        }

        _productRepository.Update(product);

        return product.Id;
    }
}
