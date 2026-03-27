using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Queries;

public class GetAllProductQueryHandler : IRequestHandler<GetAllProductQuery, List<ProductDTO>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<ProductDTO>> Handle(GetAllProductQuery request, CancellationToken ct)
    {
        var query = _productRepository.Query().Include(x => x.Inventories).Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => EF.Functions.Like(x.Name, $"%{request.SearchTerm}%"));
        }

        var products = await query.ToListAsync(ct);

        return products.Select(x => new ProductDTO
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Price = x.Price,
            Inventories = x.Inventories.Select(y => new InventoryDTO
            {
                Id = y.Id,
                Stock = y.Stock,
                Name = y.Name
            }).ToList()
        }).ToList();
    }
}
