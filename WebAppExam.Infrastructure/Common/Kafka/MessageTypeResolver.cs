using System;
using KafkaFlow;
using KafkaFlow.Middlewares.Serializer.Resolvers;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Events;

namespace WebAppExam.API.Common.Kafka;

public class MessageTypeResolver : IMessageTypeResolver
{
    private static readonly Dictionary<string, Type> _types = new()
    {
        { "OrderCreatedEvent", typeof(OrderCreatedEvent) },
        { "OrderUpdatedEvent", typeof(OrderUpdatedEvent) }
    };
    public ValueTask<Type?> OnConsumeAsync(IMessageContext context)
    {
        var typeName = context.Headers.GetString("Message-Type");

        Console.WriteLine($"[Kafka-Resolver] Đang phiên dịch Type: '{typeName}'");

        if (typeName != null && _types.TryGetValue(typeName, out var type))
        {
            Console.WriteLine($"[Kafka-Resolver] Đã tìm thấy map cho: {type.Name}");
            return ValueTask.FromResult<Type?>(type);
        }

        Console.WriteLine($"[Kafka-Resolver] KHÔNG TÌM THẤY MAP CHO: '{typeName}'");
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

            // 3. Đóng dấu tên ngắn vào Header
            context.Headers.Add("Message-Type", System.Text.Encoding.UTF8.GetBytes(aliasName));
        }

        return ValueTask.CompletedTask;
    }
}
