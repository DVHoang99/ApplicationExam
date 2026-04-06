using System;
using System.Runtime.Serialization;
using System.Windows.Input;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Orders.Commands;

public class UpdateOrderCommand(Ulid id) : ICommand<Ulid>
{
    public Ulid Id { get; set; } = id;
    public Ulid CustomerId { get; set; }
    public required string Address { get; set; }
    public required string PhoneNumber { get; set; }
    public required string CustomerName { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public static UpdateOrderCommand Init(Ulid id, OrderDto input)
    {
        return new UpdateOrderCommand(id)
        {
            CustomerName = input.CustomerName,
            Address = input.Address,
            PhoneNumber = input.PhoneNumber,
            Items = [.. input.Details.Select(x => new OrderItemDto
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                WareHouseId = x.WareHouseId
            })]
        };
    }
}
