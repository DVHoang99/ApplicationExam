namespace WebAppExam.Application.Orders.DTOs;

public class OrderItemDTO
{
    public Ulid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public required string WareHouseId { get; set; }

    public static OrderItemDTO Init(Ulid productId, int quantity, string wareHouseId)
    {
        return new OrderItemDTO
        {
            ProductId = productId,
            Quantity = quantity,
            WareHouseId = wareHouseId
        };
    }
}
