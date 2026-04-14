using System;
using KafkaFlow;
using KafkaFlow.Middlewares.Serializer.Resolvers;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Events;
using Microsoft.Extensions.Logging;

namespace WebAppExam.API.Common.Kafka;

public class MessageTypeResolver : IMessageTypeResolver
{
    private static readonly Dictionary<string, Type> _types = new()
    {
        { "OrderCreatedEvent", typeof(OrderCreatedEvent) },
        { "OrderUpdatedEvent", typeof(OrderUpdatedEvent) },
        { "OrderCanceledEvent", typeof(OrderCanceledEvent) },
        { "OrderDeletedEvent", typeof(OrderDeletedEvent) }
    };
    private readonly ILogger<MessageTypeResolver> _logger;

    public MessageTypeResolver(ILogger<MessageTypeResolver> logger)
    {
        _logger = logger;
    }
    public ValueTask<Type?> OnConsumeAsync(IMessageContext context)
    {
        var typeName = context.Headers.GetString("Message-Type");

        _logger.LogDebug("[Kafka-Resolver] Đang phiên dịch Type: '{TypeName}'", typeName);

        if (typeName != null && _types.TryGetValue(typeName, out var type))
        {
            _logger.LogDebug("[Kafka-Resolver] Đã tìm thấy map cho: {TypeName}", type.Name);
            return ValueTask.FromResult<Type?>(type);
        }

        _logger.LogWarning("[Kafka-Resolver] KHÔNG TÌM THẤY MAP CHO: '{TypeName}'", typeName);
        return ValueTask.FromResult<Type?>(null);
    }

    public ValueTask OnProduceAsync(IMessageContext context)
    {
        // 1. Lấy cái VALUE thực sự nằm bên trong vỏ bọc
        var realPayload = context.Message.Value;

        if (realPayload != null)
        {
            var type = realPayload.GetType();

            // Mặc định lấy tên dài
            string aliasName = type.FullName!;

            // 2. ÉP TÊN DÀI THÀNH TÊN NGẮN TẠI ĐÂY
            if (type == typeof(OrderCreatedEvent))
            {
                aliasName = nameof(OrderCreatedEvent);
            }
            else if (type == typeof(OrderUpdatedEvent))
            {
                aliasName = nameof(OrderUpdatedEvent);
            }
            else if (type == typeof(OrderCanceledEvent))
            {
                aliasName = nameof(OrderCanceledEvent);
            }
            else if (type == typeof(OrderDeletedEvent))
            {
                aliasName = nameof(OrderDeletedEvent);
            }

            // 3. Đóng dấu tên ngắn vào Header
            context.Headers.Add("Message-Type", System.Text.Encoding.UTF8.GetBytes(aliasName));
        }

        return ValueTask.CompletedTask;
    }
}
