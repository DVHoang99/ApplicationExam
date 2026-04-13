using FluentResults;
using MediatR;
using WebAppExam.Application.Orders.Services;

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
