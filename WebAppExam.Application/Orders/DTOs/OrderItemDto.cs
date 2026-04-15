namespace WebAppExam.Application.Orders.DTOs;

public class OrderItemDTO
{
    public Ulid ProductId { get; }
    public int Quantity { get;}
    public string WareHouseId { get; private set; }

    public OrderItemDTO()
    {
        
    }

}
