using FluentResults;
using MediatR;
using WebAppExam.Application.Products.Services;

namespace WebAppExam.Application.Products.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<Ulid>>
{
    private readonly IProductService _productService;
    public DeleteProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<Ulid>> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        return await _productService.DeleteProductAsync(request.ProductId, request.WareHouseId, ct);
    }
}
