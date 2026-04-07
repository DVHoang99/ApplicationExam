using FluentResults;
using MediatR;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Queries;

public class GetAllProductQueryHandler : IRequestHandler<GetAllProductQuery, Result<List<ProductDTO>>>
{
    private readonly IProductService _productService;

    public GetAllProductQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<List<ProductDTO>>> Handle(GetAllProductQuery request, CancellationToken ct)
    {
        return await _productService.GetAllProductsAsync(request.SearchTerm, request.PageNumber, request.PageSize, ct);
    }
}
