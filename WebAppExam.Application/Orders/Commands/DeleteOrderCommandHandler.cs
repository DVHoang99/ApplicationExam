using FluentResults;
using MediatR;
using WebAppExam.Application.Common;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Application.Orders.Services;
using WebAppExam.Domain.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Result<Ulid>>
{
    private readonly IOrderService _orderService;

    public DeleteOrderCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<Ulid>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.DeleteOrderAsync(request.Id, cancellationToken);
    }
}