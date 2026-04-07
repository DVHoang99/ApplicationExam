using FluentResults;
using MediatR;
using WebAppExam.Application.Orders.Services;
namespace WebAppExam.Application.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Ulid>>
{
    private readonly IOrderService _orderService;

    public CreateOrderCommandHandler(
        IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<Ulid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.CreateOrderAsync(request.CustomerId, request.Address, request.PhoneNumber, request.CustomerName, request.Items, cancellationToken);
    }
}