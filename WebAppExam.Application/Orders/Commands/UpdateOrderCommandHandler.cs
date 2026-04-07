using FluentResults;
using MediatR;
using WebAppExam.Application.Common;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Application.Orders.Services;
using WebAppExam.Domain;
using WebAppExam.Domain.Enum;
using WebAppExam.Domain.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Result<Ulid>>
{
    private readonly IOrderService _orderService;

    public UpdateOrderCommandHandler(
    IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<Ulid>> Handle(UpdateOrderCommand request, CancellationToken ct)
    {
        return await _orderService.UpdateOrderAsync(request.Id, request.CustomerId, request.CustomerName, request.Address, request.PhoneNumber, request.Items, ct);
    }
}
