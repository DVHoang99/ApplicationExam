using System;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Ulid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICacheService _cacheService;


    public DeleteOrderCommandHandler(IOrderRepository orderRepository, ICacheService cacheService)
    {
        _orderRepository = orderRepository;
        _cacheService = cacheService;
    }

    public async Task<Ulid> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);

        if (order == null)
            throw new Exception("Order not found");

        _orderRepository.Remove(order);
        await _cacheService.RemoveByPrefixAsync($"order_detail:{request.Id}");
        return order.Id;
    }
}
