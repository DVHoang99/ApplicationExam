using System;
using MediatR;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Ulid>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    public async Task<Ulid> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, ct);

        if (product == null)
            throw new Exception("Product not found");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;

        foreach (var inventory in request.Inventories)
        {
            product.AddOrUpdateInventory(inventory.Id, product.Price, inventory.Stock, product.Id);
        }

        _productRepository.Update(product);

        return product.Id;
    }
}
