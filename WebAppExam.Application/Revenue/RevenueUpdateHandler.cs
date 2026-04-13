using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using WebAppExam.Application.Common;
using WebAppExam.Application.Orders.Commands;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Revenue;

public class RevenueUpdateHandler : IMessageHandler<OrderCreatedIntegrationEvent>
{
    private readonly IServiceProvider _serviceProvider;

    public RevenueUpdateHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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
        
        if (await inboxService.HasBeenProcessedAsync(messageId))
        {
            Console.WriteLine($"[Kafka] Message {messageId} already processed. Skipping.");
            return;
        }

        await uow.BeginTransactionAsync();
        try
        {
            await inboxService.CreateInboxMessageAsync(messageId, nameof(OrderCreatedIntegrationEvent));
            
            await repository.UpsertMonthlyRevenueAsync(message.OccurredOn, message.Amount, message.Counter);
            
            await inboxService.MarkAsProcessedAsync(messageId);
            
            await uow.CommitAsync();
            Console.WriteLine($"[Kafka] processing calculate monthly revenue for order {message.OrderId}");
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync();
            Console.WriteLine($"[Kafka] Error processing message {messageId}: {ex.Message}");
            throw;
        }
    }
}
