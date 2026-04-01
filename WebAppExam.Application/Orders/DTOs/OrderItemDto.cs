namespace WebAppExam.Application.Orders.DTOs;

public class OrderItemDto
{
    public Ulid ProductId { get; set; }
    public int Quantity { get; set; }
    public Ulid WareHouseId { get; set; }
}
