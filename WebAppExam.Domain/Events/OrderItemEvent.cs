using System;

namespace WebAppExam.Domain.Events;

public class OrderItemEvent
{
    public string ProductId { get; init; }
    public int Quantity { get; init; }
    public string WareHouseId { get; init; }

    public OrderItemEvent() { }

    private OrderItemEvent(string productId, int quantity, string wareHouseId)
    {
        ProductId = productId;
        Quantity = quantity;
        WareHouseId = wareHouseId;
    }

    public static OrderItemEvent Init(string productId, int quantity, string wareHouseId)
    {
        return new OrderItemEvent(productId, quantity, wareHouseId);
    }
}
