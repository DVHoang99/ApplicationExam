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
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
