using System;
using MediatR;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Ulid>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Ulid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product(request.Name, request.Description, request.Price);

        foreach (var inventory in request.Inventories)
        {
            product.AddOrUpdateInventory(Ulid.Empty, product.Price, inventory.Stock, product.Id);
        }

        await _repository.AddAsync(product, ct);

        return product.Id;
    }
}