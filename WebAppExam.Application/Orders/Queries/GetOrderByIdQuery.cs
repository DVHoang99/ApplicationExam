using FluentResults;
using MediatR;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Queries;

public record GetOrderByIdQuery(Ulid id) : IRequest<Result<OrderDTO>>
{
    public Ulid Id { get; private set; } = id;
    public static GetOrderByIdQuery Init(Ulid id)
    {
        return new GetOrderByIdQuery(id);
    }
}

