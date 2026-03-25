using MediatR;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Commands;

public class CreateOrderCommand: IRequest<Ulid>
{
    public Ulid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}