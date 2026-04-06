using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Orders.Commands;

public class CreateOrderCommand : ICommand<Ulid>
{
    public Ulid CustomerId { get; set; }
    public required string Address { get; set; }
    public required string PhoneNumber { get; set; }
    public required string CustomerName { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public static CreateOrderCommand Init(OrderDto input)
    {
        return new CreateOrderCommand
        {
            CustomerId = input.CustomerId,
            Address = input.Address,
            PhoneNumber = input.PhoneNumber,
            CustomerName = input.CustomerName,
            Items = input.Details.Select(x => new OrderItemDto
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                WareHouseId = x.WareHouseId
            }).ToList()
        };
    }
}