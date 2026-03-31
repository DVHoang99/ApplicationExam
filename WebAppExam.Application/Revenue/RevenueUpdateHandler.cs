using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using WebAppExam.Application.Orders.Commands;
using WebAppExam.Application.Orders.Events;

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

        var repository = scope.ServiceProvider.GetRequiredService<IRevenueRepository>();

        await repository.UpsertMonthlyRevenueAsync(message.OccurredOn, message.Amount, message.Counter);

        Console.WriteLine($"[Kafka] processing calculate monthly revenue for order {message.OrderId}");
    }
}
