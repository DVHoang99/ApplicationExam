using System;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Queries;
public class GetAllOrdersQuery(DateTime fromDate, DateTime toDate, string customerName, string phoneNumber) : IRequest<List<OrderDto>>
{
    public string CustomerName { get; set; } = customerName;
    public string PhoneNumber { get; set; } = phoneNumber;
    public DateTime FromDate { get; set; } = fromDate;
    public DateTime ToDate { get; set; } = toDate;
}
