using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebAppExam.Application.Common;
using WebAppExam.Application.Orders.Commands;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Revenue;

public class RevenueUpdateHandler : IMessageHandler<OrderCreatedIntegrationEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RevenueUpdateHandler> _logger;

    public RevenueUpdateHandler(IServiceProvider serviceProvider, ILogger<RevenueUpdateHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Handle(IMessageContext context, OrderCreatedIntegrationEvent message)
    {
        using var scope = _serviceProvider.CreateScope();

        var inboxService = scope.ServiceProvider.GetRequiredService<IInboxService>();
        var repository = scope.ServiceProvider.GetRequiredService<IRevenueRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Idempotency check using Inbox Pattern
        // Assuming OrderId or a combination of fields serves as a unique message identifier
        var messageId = $"revenue-update:{message.OrderId}";
        
        try
        {
            if (await inboxService.HasBeenProcessedAsync(messageId))
            {
                _logger.LogInformation("[Kafka] Message {MessageId} already processed. Skipping.", messageId);
                return;
            }
        }
        catch (StackExchange.Redis.RedisException redisEx)
        {
            _logger.LogError(redisEx, "[Kafka] Redis connection failed while checking idempotency for {MessageId}", messageId);
            throw new WebAppExam.Infrastructure.Exceptions.TransientOperationException("Redis cache operation failed", redisEx);
        }

        await uow.BeginTransactionAsync();
        try
        {
            await inboxService.CreateInboxMessageAsync(messageId, nameof(OrderCreatedIntegrationEvent));
            
            await repository.UpsertMonthlyRevenueAsync(message.OccurredOn, message.Amount, message.Counter);
            
            await inboxService.MarkAsProcessedAsync(messageId);
            
            await uow.CommitAsync();
            _logger.LogInformation("[Kafka] processing calculate monthly revenue for order {OrderId}", message.OrderId);
        }
        catch (StackExchange.Redis.RedisException redisEx)
        {
            await uow.RollbackAsync();
            _logger.LogError(redisEx, "[Kafka] Redis Error processing message {MessageId}", messageId);
            // Wrap in TransientOperationException so KafkaFlow retry policy triggers
            throw new WebAppExam.Infrastructure.Exceptions.TransientOperationException("Redis operation failed", redisEx);
        }
        catch (Exception dbEx) when (dbEx.GetType().Name == "DbUpdateException" || dbEx.GetType().Name == "PostgresException")
        {
            await uow.RollbackAsync();
            _logger.LogError(dbEx, "[Kafka] Database Error processing message {MessageId}", messageId);
            // Wrap in TransientOperationException so KafkaFlow retry policy triggers
            throw new WebAppExam.Infrastructure.Exceptions.TransientOperationException("Database commit failed", dbEx);
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync();
            _logger.LogError(ex, "[Kafka] Unknown Error processing message {MessageId}", messageId);
            throw;
        }
    }
}
