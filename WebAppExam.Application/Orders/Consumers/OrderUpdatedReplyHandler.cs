using System;
using System.Drawing;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Consumers;

public class OrderUpdatedReplyHandler : IMessageHandler<OrderReplyDTO>
{
    private readonly IServiceProvider _serviceProvider;

    public OrderUpdatedReplyHandler(IServiceProvider serviceProvider)
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

        if (messageDto.Status == Domain.Enum.OrderStatus.Pending)
        {
            order.UpdateOrderStatus(messageDto.Status, messageDto.Reason);
        }
        else
        {
            order.UpdateOrderStatus(Domain.Enum.OrderStatus.Pending, messageDto.Reason);
            await RollbackOrder(messageDto, order);
        }

        repository.Update(order);


        var key = order.CreatedAt.Date.ToString("yyyy-MM-dd");
        var dailyRevenue = await dailyRepository.GetByKeyAsync(key, CancellationToken.None);
        if (dailyRevenue != null && dailyRevenue.UpdatedAt > order.CreatedAt)
        {
            dailyRevenue.AddDailyRevenue(0, -order.TotalAmount);
            dailyRepository.Update(dailyRevenue);
        }

        await uow.CommitAsync();
    }

    private async Task RollbackOrder(OrderReplyDTO messageDto, Order order)
    {
        foreach (var item in messageDto.Data)
        {
            order.AddOrUpdateItem(item.ProductId, item.Price, item.Quantity, Ulid.Parse(item.WareHouseId));
        }
    }

}
