using System;
using Confluent.Kafka;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Queries;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDTO>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;


    public GetProductByIdQueryHandler(IProductRepository productRepository, ICacheService cacheService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
    }

    public async Task<ProductDTO> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await _cacheService.GetAsync($"product_detail:{request.Id}", async () =>
        {
            var res = await _productRepository.GetByIdAsync(request.ProductId, ct);

            if (res == null)
            {
                var failure = new FluentValidation.Results.ValidationFailure("Product", "Product not found");
                throw new FluentValidation.ValidationException(new[] { failure });
            }

            return new ProductDTO
            {
                Id = res.Id,
                Name = res.Name,
                Description = res.Description,
                Price = res.Price,
                Inventories = res.Inventories.Select(x => new InventoryDTO
                {
                    Id = x.Id,
                    Stock = x.Stock,
                    Name = x.Name
                }).ToList()
            };
        }, TimeSpan.FromDays(1), ct);

        return product ?? new ProductDTO();
    }
}
