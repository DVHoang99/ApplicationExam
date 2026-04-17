using System.Text.Json;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebAppExam.Application.Common;
using WebAppExam.Application.Orders.Commands;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Repository;
using WebAppExam.GrpcContracts.Protos;
using WebAppExam.Application.OutboxMessages.DTOs;

namespace WebAppExam.Application.Revenue;

public class RevenueUpdateHandler : IMessageHandler<OutboxPointer>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RevenueUpdateHandler> _logger;
    private readonly OutboxGrpc.OutboxGrpcClient _outboxClient;

    public RevenueUpdateHandler(IServiceProvider serviceProvider, ILogger<RevenueUpdateHandler> logger, OutboxGrpc.OutboxGrpcClient outboxClient)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _outboxClient = outboxClient;
    }

    public async Task Handle(IMessageContext context, OutboxPointer pointer)
    {
        if (pointer.Type != nameof(OrderCreatedIntegrationEvent))
        {
            return; // Only handle OrderCreatedIntegrationEvent here
        }

        // 1. Fetch full message content via gRPC
        var response = await _outboxClient.GetOutboxMessageAsync(new OutboxMessageRequest { Id = pointer.Id });
        var message = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(response.Content);

        if (message == null)
        {
            _logger.LogError("[Kafka] Failed to deserialize OrderCreatedIntegrationEvent from gRPC content for Outbox {Id}", pointer.Id);
            return;
        }

        using var scope = _serviceProvider.CreateScope();

        var inboxService = scope.ServiceProvider.GetRequiredService<IInboxService>();
        var repository = scope.ServiceProvider.GetRequiredService<IRevenueRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Idempotency check using Inbox Pattern
        var messageId = $"revenue-update:{message.OrderId}";
        
        try
        {
            if (await inboxService.HasBeenProcessedAsync(messageId))
            {
                _logger.LogInformation("[Kafka] Message {MessageId} already processed. Skipping.", messageId);
                // Optionally mark as completed even if skipped due to idempotency
                await _outboxClient.UpdateOutboxStatusAsync(new UpdateStatusRequest { Id = pointer.Id, Status = 3 }); // Completed
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Error checking idempotency for {MessageId}", messageId);
            throw;
        }

        await uow.BeginTransactionAsync();
        try
        {
            await inboxService.CreateInboxMessageAsync(messageId, nameof(OrderCreatedIntegrationEvent));
            await repository.UpsertMonthlyRevenueAsync(message.OccurredOn, message.Amount, message.Counter);
            await inboxService.MarkAsProcessedAsync(messageId);
            
            await uow.CommitAsync();
            _logger.LogInformation("[Kafka] processing calculate monthly revenue for order {OrderId}", message.OrderId);

            // FEEDBACK: Success
            await _outboxClient.UpdateOutboxStatusAsync(new UpdateStatusRequest { 
                Id = pointer.Id, 
                Status = 3 // Completed
            });
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync();
            _logger.LogError(ex, "[Kafka] Error processing message {MessageId}", messageId);

            // FEEDBACK: Failure
            await _outboxClient.UpdateOutboxStatusAsync(new UpdateStatusRequest { 
                Id = pointer.Id, 
                Status = 2, // Failed
                Error = ex.Message
            });

            throw;
        }
    }
}
