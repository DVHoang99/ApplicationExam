using FluentResults;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Orders.Commands;

public class UpdateOrderCommand(
    Ulid id, 
    Ulid customerId, 
    string customerName, 
    string address, 
    string phoneNumber, 
    List<OrderItemDTO> items) : ICommand<Result<Ulid>>
{
    public Ulid Id { get; private set; } = id;
    public Ulid CustomerId { get; private set; } = customerId;
    public string Address { get; private set; } = address;
    public string PhoneNumber { get; private set; } = phoneNumber;
    public string CustomerName { get; private set; } = customerName;
    public List<OrderItemDTO> Items { get; private set; } = items;
    public static UpdateOrderCommand Init(Ulid id, OrderDTO input)
    {
        var orderItemDTO = input.Details.Select(x => new OrderItemDTO(x.ProductId, x.Quantity, x.WareHouseId)).ToList();

        return new UpdateOrderCommand(id, input.CustomerId, input.CustomerName, input.Address, input.PhoneNumber, orderItemDTO);
    }
}
