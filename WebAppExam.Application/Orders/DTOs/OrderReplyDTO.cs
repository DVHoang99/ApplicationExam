using System;
using System.Security;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Application.Orders.DTOs;

public class OrderReplyDTO
{
    public Ulid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string Reason { get; set; }
    public List<OrderDetailDto> Data { get; set; } = new();

}
