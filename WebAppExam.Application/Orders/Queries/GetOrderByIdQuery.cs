using MediatR;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Queries;

public record GetOrderByIdQuery(Ulid Id) : IRequest<OrderDto>
{
    public Ulid Id { get; set; } = Id;
    public static GetOrderByIdQuery Init(Ulid id)
    {
        return new GetOrderByIdQuery(id);
    }
}

