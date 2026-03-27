using System;
using MediatR;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Queries;

[CacheQuery(prefix: "orders_list", expirationMinutes: 5)]
public class GetAllOrdersQuery(string customerName) : IRequest<List<OrderDto>>
{
    public string CustomerName { get; set; } = customerName;
}
