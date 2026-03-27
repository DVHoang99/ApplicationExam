namespace WebAppExam.Application.Orders.DTOs;

public class OrderDetailDto
{
    public Ulid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }
    public Ulid InventoryId { get; set; }

    public decimal SubTotal => Quantity * Price;
}