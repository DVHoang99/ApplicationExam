namespace WebAppExam.Application.Orders.DTOs;

public class OrderItemDTO
{
    public Ulid ProductId { get; }
    public int Quantity { get;}
    public string WareHouseId { get; private set; }

    public OrderItemDTO()
    {
        
    }

    public OrderItemDTO(Ulid productId, int quantity, string wareHouseId)
    {
        ProductId = productId;
        Quantity = quantity;
        WareHouseId = wareHouseId;
    }

}
