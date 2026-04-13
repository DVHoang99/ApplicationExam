using System;
using KafkaFlow;
using KafkaFlow.Middlewares.Serializer.Resolvers;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Events;

namespace WebAppExam.Infrastructure.Common.Kafka;

public class InventoryMessageTypeResolver : IMessageTypeResolver
{
    private static readonly Dictionary<string, Type> _types = new()
    {
        { "WebAppExam.Application.Orders.Events.OrderCreatedEvent", typeof(OrderCreatedEvent) },
        { "WebAppExam.Application.Orders.Events.OrderUpdatedEvent", typeof(OrderUpdatedEvent) },
        { "WebAppExam.Domain.Events.OrderCanceledEvent", typeof(OrderCanceledEvent) },
        { "WebAppExam.Domain.Events.OrderDeletedEvent", typeof(OrderDeletedEvent) },
    };

    public ValueTask<Type?> OnConsumeAsync(IMessageContext context)
    {
        var typeName = context.Headers.GetString("Message-Type");

        if (typeName != null && _types.TryGetValue(typeName, out var type))
        {
            return ValueTask.FromResult<Type?>(type);
        }

        // Hết cảnh báo vàng
        return ValueTask.FromResult<Type?>(null);
    }

    public ValueTask OnProduceAsync(IMessageContext context)
    {
        var type = context.Message.GetType();
        context.Headers.Add("Message-Type", System.Text.Encoding.UTF8.GetBytes(type.FullName!));

        return ValueTask.CompletedTask;
    }
}
