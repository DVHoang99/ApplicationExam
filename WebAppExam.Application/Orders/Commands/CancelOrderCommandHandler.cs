using FluentResults;
using MediatR;
using WebAppExam.Application.Orders.Services;

namespace WebAppExam.Application.Orders.Commands;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<Ulid>>
{
    private readonly IOrderService _orderService;

    public CancelOrderCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<Ulid>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.CancelOrderAsync(request.Id, cancellationToken);
    }
}
