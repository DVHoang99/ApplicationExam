using System;

namespace WebAppExam.Domain.Events;

public class OrderItemEvent
{
    public string ProductId { get; private set; }
    public int Quantity { get; private set; }
    public string WareHouseId { get; private set; }

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
