using MediatR;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Queries;

public record GetOrderByIdQuery(Ulid Id) : IRequest<OrderDto>;