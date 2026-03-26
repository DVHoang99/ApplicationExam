namespace WebAppExam.Domain;

public class OrderDetail : EntityBase
{
    public Ulid OrderId { get; set; }
    public Ulid ProductId { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    protected OrderDetail() { }

    internal OrderDetail(Ulid productId, int unitPrice, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity greater than 0 is required");
        if (unitPrice < 0) throw new ArgumentException("Price greater than 0 is required");

        Id = Ulid.NewUlid();
        ProductId = productId;
        Price = unitPrice;
        Quantity = quantity;
    }

    internal void UpdateQuantity(int additionalQuantity)
    {
        Quantity += additionalQuantity;
    }
}