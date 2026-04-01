using System;
using MediatR;
using WebAppExam.Application.Products.Services;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Ulid>
{
    private readonly IProductRepository _repository;
    private readonly IWareHouseService _wareHouseService;
    private readonly IInventoryService _inventoryService;
    private readonly IUnitOfWork _uow;


    public CreateProductCommandHandler(IProductRepository repository, IWareHouseService wareHouseService, IInventoryService inventoryService, IUnitOfWork uow)
    {
        _repository = repository;
        _wareHouseService = wareHouseService;
        _inventoryService = inventoryService;
        _uow = uow;
    }

    public async Task<Ulid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();

        var product = new Product(request.Name, request.Description, request.Price, correlationId, request.WareHouseId);

        var wareHouse = await _wareHouseService.GetWareHouseAsync(request.WareHouseId, ct);

        if (wareHouse == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("WareHouse", "WareHouse not found.");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        await _repository.AddAsync(product, ct);

        await _uow.SaveChangesAsync(ct);

        var createInventoryAsync = await _inventoryService.CreateInventoryAsync(wareHouse.Id, product.Id.ToString(), request.Stock, correlationId, ct);

        if (createInventoryAsync != null)
        {
            product.UpdateProductStatus(Domain.Enum.ProductStatus.Active);
            _repository.Update(product);
        }

        return product.Id;
    }
}