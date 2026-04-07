using System;
using System.Security;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Application.Orders.DTOs;

public class OrderReplyDTO
{
    public Ulid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string Reason { get; private set; }
    public List<OrderDetailDTO> Data { get; private set; } = new();

}
