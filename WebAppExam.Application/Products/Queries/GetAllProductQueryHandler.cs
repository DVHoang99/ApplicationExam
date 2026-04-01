using MediatR;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Queries;

public class GetAllProductQueryHandler : IRequestHandler<GetAllProductQuery, List<ProductDTO>>
{
    private readonly IProductRepository _productRepository;
    private readonly IInventoryService _inventoryService;



    public GetAllProductQueryHandler(IProductRepository productRepository, IInventoryService inventoryService)
    {
        _productRepository = productRepository;
        _inventoryService = inventoryService;
    }

    public async Task<List<ProductDTO>> Handle(GetAllProductQuery request, CancellationToken ct)
    {
        var query = _productRepository.Query();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = _productRepository.SearchProductNameQuery(query, request.SearchTerm);
        }

        query = query.Skip((request.pageNumber - 1) * request.pageSize).Take(request.pageSize);

        var products = await _productRepository.ToListAsync(query, ct);

        var correlationIds = products.Select(x => x.CorrelationId).ToList();

        var inventories = await _inventoryService.GetInventoryDTOsAsync(correlationIds, ct);

        var inventoriesDictionary = inventories.Count > 0 ? inventories.ToDictionary(x => x.CorrelationId, x => x) : null;

        return products.Select(x =>
        {
            var inventory = inventoriesDictionary != null && inventoriesDictionary.ContainsKey(x.CorrelationId) ? inventoriesDictionary[x.CorrelationId] : null;

            return new ProductDTO
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                WareHouseId = x.WareHouseId,
                Stock = inventory == null ? 0 : inventory.StockQuantity,
                WareHouse = inventory == null ? new WareHouseDTO() : new WareHouseDTO
                {
                    Id = inventory.WareHouseId,
                    OwerName = inventory.WareHouse.OwerName,
                    Address = inventory.WareHouse.Address,
                    OwerPhone = inventory.WareHouse.OwerPhone,
                    OwerEmail = inventory.WareHouse.OwerEmail
                }
            };
        }).ToList();

    }

}
