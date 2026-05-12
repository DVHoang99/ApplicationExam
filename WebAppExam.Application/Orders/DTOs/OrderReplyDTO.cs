using System;
using System.Security;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Application.Orders.DTOs;

public class OrderReplyDTO
{
    public Ulid OrderId { get; init; }
    public OrderStatus Status { get; init; }
    public string Reason { get; init; }
    public string Action { get; init; }
    public string IdenpotencyId { get; init; }
    public OrderDetailDTO Data { get; init; }

}
