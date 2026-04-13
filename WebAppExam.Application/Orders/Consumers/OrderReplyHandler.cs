using System;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;
using Microsoft.Extensions.Logging;
using WebAppExam.Application.Common;
using WebAppExam.Domain.Entity;
using WebAppExam.Application.Common.Caching;

namespace WebAppExam.Application.Orders.Consumers;

public class OrderReplyHandler : IMessageHandler<OrderReplyDTO>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderReplyHandler> _logger;
    private readonly ICacheService _cacheService;

    public OrderReplyHandler(IServiceProvider serviceProvider, ILogger<OrderReplyHandler> logger, ICacheService cacheService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(IMessageContext context, OrderReplyDTO messageDto)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var inboxService = scope.ServiceProvider.GetRequiredService<IInboxService>();
        var dailyRepository = scope.ServiceProvider.GetRequiredService<IDailyRevenueRepository>();
        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var inboxMessageRepository = scope.ServiceProvider.GetRequiredService<IInboxMessageRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var messageId = messageDto.IdenpotencyId;

        if (await inboxService.HasBeenProcessedAsync(messageId))
        {
            _logger.LogInformation("Message {MessageId} already processed. Skipping.", messageId);
            return;
        }

        await uow.BeginTransactionAsync();
        try
        {

            var inboxMessage = await inboxService.CreateInboxMessageAsync(messageId, nameof(OrderReplyDTO));

            switch (messageDto.Action?.ToLower())
            {
                case "created":
                    await Created(messageDto, repository, uow, inboxMessage);
                    break;

                case "updated":
                    await Updated(messageDto, repository, uow, dailyRepository, inboxMessage);
                    break;

                case "canceled":
                    await Canceled(messageDto, repository, uow, dailyRepository, inboxMessage);
                    break;

                default:
                    Console.WriteLine($"Unknown action received: {messageDto.Action}");
                    break;
            }

            if (!inboxMessage.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
            {
                inboxMessage.MarkAsProcessed();
            }

            await uow.CommitAsync();
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync();
            _logger.LogError(ex, "Error processing order reply message {MessageId}", messageId);
            throw;
        }
    }

    private async Task Created(OrderReplyDTO messageDto, IOrderRepository repository, IUnitOfWork uow, InboxMessage inboxMessage)
    {
        var order = await repository.GetOrderByIdAndStatusAsync(messageDto.OrderId, Domain.Enum.OrderStatus.Draft, CancellationToken.None);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found or not in expected status (Draft). Action: {Action}", messageDto.OrderId, messageDto.Action);
            inboxMessage.MarkAsFailed("Order not found or not in expected status (Draft)");
            return;
        }
        order.UpdateOrderStatus(messageDto.Status, messageDto.Reason);
        repository.Update(order);
    }

    private async Task Updated(OrderReplyDTO messageDto, IOrderRepository repository, IUnitOfWork uow, IDailyRevenueRepository dailyRevenueRepository, InboxMessage inboxMessage)
    {
        var order = await repository.GetOrderByIdAndStatusAsync(messageDto.OrderId, Domain.Enum.OrderStatus.Updating, CancellationToken.None);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found or not in expected status (Updating). Action: {Action}", messageDto.OrderId, messageDto.Action);
            inboxMessage.MarkAsFailed("Order not found or not in expected status (Updating)");
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
    }
    private void RollbackOrder(OrderReplyDTO messageDto, Order order)
    {
        foreach (var item in messageDto.Data)
        {
            order.AddOrUpdateItem(item.ProductId, item.Price, item.Quantity, Ulid.Parse(item.WareHouseId));
        }
    }
    private async Task Canceled(OrderReplyDTO messageDto, IOrderRepository repository, IUnitOfWork uow, IDailyRevenueRepository dailyRevenueRepository, InboxMessage inboxMessage)
    {
        var order = await repository.GetOrderByIdAndStatusAsync(messageDto.OrderId, Domain.Enum.OrderStatus.Updating, CancellationToken.None);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found or not in expected status (Updating). Action: {Action}", messageDto.OrderId, messageDto.Action);
            inboxMessage.MarkAsFailed("Order not found or not in expected status (Updating)");
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
    }
}

