using System;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;
using Microsoft.Extensions.Logging;

namespace WebAppExam.Application.Orders.Consumers;

public class OrderReplyHandler : IMessageHandler<OrderReplyDTO>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderReplyHandler> _logger;

    public OrderReplyHandler(IServiceProvider serviceProvider, ILogger<OrderReplyHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Handle(IMessageContext context, OrderReplyDTO messageDto)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dailyRepository = scope.ServiceProvider.GetRequiredService<IDailyRevenueRepository>();
        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        switch (messageDto.Action?.ToLower())
        {
            case "created":
                await Created(messageDto, repository, uow);
                break;

            case "updated":
                await Updated(messageDto, repository, uow, dailyRepository);
                break;

            case "canceled":
                await Canceled(messageDto, repository, uow, dailyRepository);
                break;

            default:
                Console.WriteLine($"Unknown action received: {messageDto.Action}");
                break;
        }
    }

    private async Task Created(OrderReplyDTO messageDto, IOrderRepository repository, IUnitOfWork uow)
    {
        var order = await repository.GetOrderByIdAndStatusAsync(messageDto.OrderId, Domain.Enum.OrderStatus.Draft, CancellationToken.None);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found or not in expected status (Draft). Action: {Action}", messageDto.OrderId, messageDto.Action);
            return;
        }
        order.UpdateOrderStatus(messageDto.Status, messageDto.Reason);
        repository.Update(order);
        await uow.CommitAsync();
    }

    private async Task Updated(OrderReplyDTO messageDto, IOrderRepository repository, IUnitOfWork uow, IDailyRevenueRepository dailyRevenueRepository)
    {
        var order = await repository.GetOrderByIdAndStatusAsync(messageDto.OrderId, Domain.Enum.OrderStatus.Updating, CancellationToken.None);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found or not in expected status (Updating). Action: {Action}", messageDto.OrderId, messageDto.Action);
            return;
        }

        if (messageDto.Status == Domain.Enum.OrderStatus.Pending)
        {
            order.UpdateOrderStatus(messageDto.Status, messageDto.Reason);
        }
        else
        {
            order.UpdateOrderStatus(Domain.Enum.OrderStatus.Pending, messageDto.Reason);
            RollbackOrder(messageDto, order);
        }

        repository.Update(order);


        var key = order.CreatedAt.Date.ToString("yyyy-MM-dd");
        var dailyRevenue = await dailyRevenueRepository.GetByKeyAsync(key, CancellationToken.None);
        if (dailyRevenue != null && dailyRevenue.UpdatedAt > order.CreatedAt)
        {
            dailyRevenue.AddDailyRevenue(0, -order.TotalAmount);
            dailyRevenueRepository.Update(dailyRevenue);
        }

        await uow.CommitAsync();
    }
    private void RollbackOrder(OrderReplyDTO messageDto, Order order)
    {
        foreach (var item in messageDto.Data)
        {
            order.AddOrUpdateItem(item.ProductId, item.Price, item.Quantity, Ulid.Parse(item.WareHouseId));
        }
    }
    private async Task Canceled(OrderReplyDTO messageDto, IOrderRepository repository, IUnitOfWork uow, IDailyRevenueRepository dailyRevenueRepository)
    {
        var order = await repository.GetOrderByIdAndStatusAsync(messageDto.OrderId, Domain.Enum.OrderStatus.Updating, CancellationToken.None);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found or not in expected status (Updating). Action: {Action}", messageDto.OrderId, messageDto.Action);
            return;
        }

        order.UpdateOrderStatus(messageDto.Status, messageDto.Reason);

        repository.Update(order);

        var key = order.CreatedAt.Date.ToString("yyyy-MM-dd");
        var dailyRevenue = await dailyRevenueRepository.GetByKeyAsync(key, CancellationToken.None);
        if (dailyRevenue != null && dailyRevenue.UpdatedAt > order.CreatedAt)
        {
            dailyRevenue.AddDailyRevenue(-1, -order.TotalAmount);
            dailyRevenueRepository.Update(dailyRevenue);
        }

        await uow.CommitAsync();
    }
}

