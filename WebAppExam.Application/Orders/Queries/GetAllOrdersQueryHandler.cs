using System;
using FluentResults;
using MediatR;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Orders.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Queries;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<List<OrderDTO>>>
{
    private readonly IOrderService _orderService;

    public GetAllOrdersQueryHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<List<OrderDTO>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _orderService.GetAllOrdersAsync(
            request.FromDate,
            request.ToDate,
            request.CustomerName,
            request.PhoneNumber,
            request.pageNumber,
            request.pageSize,
            cancellationToken);
    }
}
