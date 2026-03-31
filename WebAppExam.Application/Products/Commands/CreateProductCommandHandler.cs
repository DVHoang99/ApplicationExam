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


    public CreateProductCommandHandler(IProductRepository repository, IWareHouseService wareHouseService)
    {
        _repository = repository;
        _wareHouseService = wareHouseService;
    }

    public async Task<Ulid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product(request.Name, request.Description, request.Price);

        var wareHouseId = request.Inventories.Select(x => x.WareHouseId).First();

        var wareHouse = await _wareHouseService.GetWareHouseAsync(wareHouseId, ct);

        if (wareHouse == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("WareHouse", "WareHouse not found.");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        foreach (var inventory in request.Inventories)
        {
            product.AddOrUpdateInventory(Ulid.Empty, inventory.Stock, product.Id, inventory.Name);
        }

        await _repository.AddAsync(product, ct);



        return product.Id;
    }
}