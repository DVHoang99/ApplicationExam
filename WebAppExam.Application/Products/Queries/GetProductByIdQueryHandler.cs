using System;
using Confluent.Kafka;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Queries;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDTO>
{
    private readonly IProductRepository _productRepository;
    private readonly IInventoryService _inventoryService;


    public GetProductByIdQueryHandler(IProductRepository productRepository, IInventoryService inventoryService)
    {
        _productRepository = productRepository;
        _inventoryService = inventoryService;
    }

    public async Task<ProductDTO> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var res = await _productRepository.GetByIdAsync(request.ProductId, ct);

        if (res == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Product", "Product not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        var inventories = await _inventoryService.GetInventoryDTOsAsync(new List<string> { res.CorrelationId }, ct);

        var inventoriesDictionary = inventories.Count > 0 ? inventories.ToDictionary(x => x.CorrelationId, x => x) : null;

        return new ProductDTO
        {
            Id = res.Id,
            Name = res.Name,
            Description = res.Description,
            Price = res.Price,
            WareHouseId = res.WareHouseId,
            Stock = inventoriesDictionary != null && inventoriesDictionary.ContainsKey(res.CorrelationId) ? inventoriesDictionary[res.CorrelationId].StockQuantity : 0,
            WareHouse = inventoriesDictionary == null ? new WareHouseDTO() : new WareHouseDTO
            {
                Id = inventoriesDictionary[res.CorrelationId].WareHouseId,
                OwerName = inventoriesDictionary[res.CorrelationId].WareHouse.OwerName,
                Address = inventoriesDictionary[res.CorrelationId].WareHouse.Address,
                OwerPhone = inventoriesDictionary[res.CorrelationId].WareHouse.OwerPhone,
                OwerEmail = inventoriesDictionary[res.CorrelationId].WareHouse.OwerEmail
            }
        };
    }
}
