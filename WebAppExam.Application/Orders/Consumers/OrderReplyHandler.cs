using System;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Consumers;

public class OrderReplyHandler : IMessageHandler<OrderReplyDTO>
{
    private readonly IServiceProvider _serviceProvider;

    public OrderReplyHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(IMessageContext context, OrderReplyDTO messageDto)
    {
        using var scope = _serviceProvider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var order = await repository.GetOrderByIdAndStatusAsync(messageDto.OrderId, Domain.Enum.OrderStatus.Draft, CancellationToken.None);
        if (order == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Order", "Order not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }
        order.UpdateOrderStatus(messageDto.Status, messageDto.Reason);
        repository.Update(order);
        await uow.CommitAsync();
    }
}

