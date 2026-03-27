using System;
using MediatR;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Queries;

public class GetAllOrdersQuery(string customerName) : IRequest<List<OrderDto>>
{
    public string CustomerName { get; set; } = customerName;
}
