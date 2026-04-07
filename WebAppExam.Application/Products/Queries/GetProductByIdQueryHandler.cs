using System;
using Confluent.Kafka;
using FluentResults;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Products.Queries;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDTO>>
{
    private readonly IProductService _productService;

    public GetProductByIdQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<ProductDTO>> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        return await _productService.GetProductByIdAsync(request.ProductId, ct);
    }
}
