using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Orders.Commands;

public class CreateOrderCommand : ICommand<Ulid>
{
    public Ulid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}