using System;
using MediatR;
using WebAppExam.Application.Products.DTOs;

namespace WebAppExam.Application.Products.Queries;

public class GetProductByIdQuery(Ulid productId) : IRequest<ProductDTO>
{
    public Ulid ProductId { get; set; } = productId;
}
