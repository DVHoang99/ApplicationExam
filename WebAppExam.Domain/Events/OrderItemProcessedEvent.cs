using System;

namespace WebAppExam.Application.Orders.Events;

public class OrderItemProcessedEvent
{
    public string OrderId { get; init; } = default!;
    public string CustomerName { get; init; } = default!;
    public string ProductId { get; init; } = default!;
    public int Quantity { get; init; }
    public string WareHouseId { get; init; } = default!;
    public string IdempotencyId { get; init; } = default!;
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;

    public OrderItemProcessedEvent() { }

    public static OrderItemProcessedEvent Create(string orderId, string customerName, string productId, int quantity, string wareHouseId, string idempotencyId)
    {
        return new OrderItemProcessedEvent
        {
            OrderId = orderId,
            CustomerName = customerName,
            ProductId = productId,
            Quantity = quantity,
            WareHouseId = wareHouseId,
            IdempotencyId = idempotencyId
        };
    }
}
