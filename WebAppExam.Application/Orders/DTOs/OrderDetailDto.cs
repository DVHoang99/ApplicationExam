namespace WebAppExam.Application.Orders.DTOs;

public class OrderDetailDto
{
    public Ulid ProductId { get; set; }

    public int Quantity { get; set; }

    public int Price { get; set; }
    public string WareHouseId { get; set; }

    public decimal SubTotal => Quantity * Price;
}