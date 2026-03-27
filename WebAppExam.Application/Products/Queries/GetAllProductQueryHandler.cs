using MediatR;
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
        var query = _productRepository
        .Include(_productRepository.Query())
        .Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = _productRepository.SearchProductNameQuery(query, request.SearchTerm);
        }

        var products = await _productRepository.ToListAsync(query, ct);

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
