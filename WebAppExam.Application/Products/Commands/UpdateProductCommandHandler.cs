using FluentResults;
using MediatR;
using WebAppExam.Application.Products.Services;

namespace WebAppExam.Application.Products.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<Ulid>>
{
    private readonly IProductService _productService;

    public UpdateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }
    public async Task<Result<Ulid>> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        return await _productService.UpdateProductAsync(request.ProductId, request.Name, request.Description, request.Price, request.WareHouseId, request.Stock, ct);
    }
}
