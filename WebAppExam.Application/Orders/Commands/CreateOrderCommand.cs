using FluentResults;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Orders.Commands;

public class CreateOrderCommand(Ulid customerId, string address, string phoneNumber, string customerName, List<OrderItemDTO> items) : ICommand<Result<Ulid>>
{
    public Ulid CustomerId { get; private set; } = customerId;
    public string Address { get; private set; } = address;
    public string PhoneNumber { get; private set; } = phoneNumber;
    public string CustomerName { get; private set; } = customerName;
    public List<OrderItemDTO> Items { get; private set; } = items;
    public static CreateOrderCommand Init(OrderDTO input)
    {
        var orderItemDTO = input.Details.Select(x => OrderItemDTO.Init(x.ProductId, x.Quantity, x.WareHouseId)).ToList();

        return new CreateOrderCommand(input.CustomerId, input.Address, input.PhoneNumber, input.CustomerName, orderItemDTO);
    }
}