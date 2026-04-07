using System;
using FluentResults;
using MediatR;
using WebAppExam.Application.Products.DTOs;

namespace WebAppExam.Application.Products.Queries;

public class GetProductByIdQuery(Ulid productId) : IRequest<Result<ProductDTO>>
{
    public Ulid ProductId { get; private set; } = productId;

    public static GetProductByIdQuery Init(Ulid productId)
    {
        return new GetProductByIdQuery(productId);
    }
}
