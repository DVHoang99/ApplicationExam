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
using StackExchange.Redis;
using WebAppExam.Application.Common.Enums;
using System.Text.Json;
using WebAppExam.Infrastructure.Exceptions;

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
        var inboxService = scope.ServiceProvider.GetRequiredService<IInboxService>();

        var messageId = $"order_reply:{messageDto.IdenpotencyId}";

        try
        {
            if (await inboxService.HasBeenProcessedAsync(messageId))
            {
                _logger.LogInformation("Message {MessageId} already processed. Skipping.", messageId);
                return;
            }
        }
        catch (StackExchange.Redis.RedisException redisEx)
        {
            _logger.LogError(redisEx, "Redis connection failed while checking idempotency for {MessageId}", messageId);
            throw new RedisException("Redis cache operation failed", redisEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed while checking idempotency for {MessageId}", messageId);
            throw;
        }

        await uow.BeginTransactionAsync();
        try
        {
            await inboxService.CreateInboxMessageAsync(messageId, "OrderReply");

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
                    _logger.LogWarning("Unknown action received: {Action}", messageDto.Action);
                    break;
            }

            await inboxService.MarkAsProcessedAsync(messageId);
            await uow.CommitAsync();
        }
        catch (StackExchange.Redis.RedisException redisEx)
        {
            await uow.RollbackAsync();
            _logger.LogError(redisEx, "Redis Error processing order reply message {MessageId}", messageId);
            throw new RedisException("Redis cache operation failed", redisEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unknown Error processing order reply message {MessageId}", messageId);
            throw;
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
    }

    private async Task Updated(OrderReplyDTO messageDto, IOrderRepository repository, IUnitOfWork uow, IDailyRevenueRepository dailyRevenueRepository)
    {
        var order = await repository.GetOrderByIdAndStatusAsync(messageDto.OrderId, Domain.Enum.OrderStatus.Updating, CancellationToken.None);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found or not in expected status (Updating). Action: {Action}", messageDto.OrderId, messageDto.Action);
            return;
        }

        if (messageDto.Status != Domain.Enum.OrderStatus.Failed)
        {
            order.UpdateOrderStatus(messageDto.Status, messageDto.Reason);
        }
        else
        {
            order.UpdateOrderStatus(messageDto.Status, messageDto.Reason);
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
    private void RollbackOrder(OrderReplyDTO messageDto, Domain.Order order)
    {
        // foreach (var item in messageDto.Data)
        // {
        //     order.RollBackItem(item.ProductId, item.Price, item.Quantity, Ulid.Parse(item.WareHouseId));
        // }

        order.RollBackItem(messageDto.Data.ProductId, messageDto.Data.Price, messageDto.Data.Quantity, Ulid.Parse(messageDto.Data.WareHouseId));
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
    }
}

