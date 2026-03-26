using System;
using MediatR;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Queries;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDTO>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDTO> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, ct);

        if (product == null)
            throw new Exception("Product not found");

        return new ProductDTO
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Inventories = product.Inventories.Select(x => new InventoryDTO
            {
                Id = x.Id,
                Stock = x.Stock
            }).ToList()
        };
    }
}
