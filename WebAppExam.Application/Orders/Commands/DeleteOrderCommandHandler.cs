using System;
using MediatR;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Ulid>
{
    private readonly IOrderRepository _orderRepository;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Ulid> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);

        if (order == null)
            throw new Exception("Order not found");

        _orderRepository.Remove(order);
        return order.Id;
    }
}
