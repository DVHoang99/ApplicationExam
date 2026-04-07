using System;
using FluentResults;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Queries;

public class GetAllOrdersQuery(
    DateTime? fromDate, 
    DateTime? toDate, 
    string customerName, 
    string phoneNumber, 
    int pageNumber, 
    int pageSize) : IRequest<Result<List<OrderDTO>>>
{
    public string CustomerName { get; private set; } = customerName;
    public string PhoneNumber { get; private set; } = phoneNumber;
    public DateTime? FromDate { get; private set; } = fromDate;
    public DateTime? ToDate { get; private set; } = toDate;
    public int pageNumber { get; private set; } = pageNumber;
    public int pageSize { get; private set; } = pageSize;
    public static GetAllOrdersQuery Init(DateTime? fromDate, DateTime? toDate, string customerName, string phoneNumber, int pageNumber, int pageSize)
    {
        return new GetAllOrdersQuery(fromDate, toDate, customerName, phoneNumber, pageNumber, pageSize);
    }
}
