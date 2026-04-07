using System;
using FluentResults;
using MediatR;
using WebAppExam.Application.Products.Services;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Ulid>>
{
    private readonly IProductService _productService;
    public CreateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<Ulid>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        return await _productService.CreateProductAsync(request.Name, request.Description, request.Price, request.WareHouseId, request.Stock, ct);
    }
}