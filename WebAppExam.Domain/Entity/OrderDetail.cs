namespace WebAppExam.Domain;

public class OrderDetail : EntityBase
{
    public Ulid OrderId { get; set; }
    public Ulid ProductId { get; set; }

    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
}