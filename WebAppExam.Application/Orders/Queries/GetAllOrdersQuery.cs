using System;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Queries;
public class GetAllOrdersQuery(DateTime? fromDate, DateTime? toDate, string customerName, string phoneNumber, int pageNumber, int pageSize) : IRequest<List<OrderDto>>
{
    public string CustomerName { get; set; } = customerName;
    public string PhoneNumber { get; set; } = phoneNumber;
    public DateTime? FromDate { get; set; } = fromDate;
    public DateTime? ToDate { get; set; } = toDate;
    public int pageNumber { get; set; } = pageNumber;
    public int pageSize { get; set; } = pageSize;
    public static GetAllOrdersQuery Init(DateTime? fromDate, DateTime? toDate, string customerName, string phoneNumber, int pageNumber, int pageSize)
    {
        return new GetAllOrdersQuery(fromDate, toDate, customerName, phoneNumber, pageNumber, pageSize);
    }
}
