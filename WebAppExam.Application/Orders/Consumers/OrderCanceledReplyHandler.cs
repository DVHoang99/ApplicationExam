using System;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Consumers;

public class OrderCanceledReplyHandler : IMessageHandler<OrderReplyDTO>
{
    private readonly IServiceProvider _serviceProvider;

    public OrderCanceledReplyHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public async Task Handle(IMessageContext context, OrderReplyDTO messageDto)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var dailyRepository = scope.ServiceProvider.GetRequiredService<IDailyRevenueRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var order = await repository.GetOrderByIdAndStatusAsync(messageDto.OrderId, Domain.Enum.OrderStatus.Updating, CancellationToken.None);
        if (order == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Order", "Order not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        order.UpdateOrderStatus(messageDto.Status, messageDto.Reason);

        repository.Update(order);

        var key = order.CreatedAt.Date.ToString("yyyy-MM-dd");
        var dailyRevenue = await dailyRepository.GetByKeyAsync(key, CancellationToken.None);
        if (dailyRevenue != null && dailyRevenue.UpdatedAt > order.CreatedAt)
        {
            dailyRevenue.AddDailyRevenue(-1, -order.TotalAmount);
            dailyRepository.Update(dailyRevenue);
        }

        await uow.CommitAsync();
    }
}
