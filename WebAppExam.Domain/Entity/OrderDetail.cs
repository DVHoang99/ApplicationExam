namespace WebAppExam.Domain;

public class OrderDetail : EntityBase
{
    public Ulid OrderId { get; set; }
    public Ulid ProductId { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    public Ulid WareHouseId { get; set; }
    protected OrderDetail() { }

    internal OrderDetail(Ulid productId, int unitPrice, int quantity, Ulid wareHouseId)
    {


        Id = Ulid.NewUlid();
        ProductId = productId;
        Price = unitPrice;
        Quantity = quantity;
        WareHouseId = wareHouseId;
        CreatedAt = DateTime.UtcNow;
    }

    internal void UpdateQuantity(int additionalQuantity)
    {
        Quantity = additionalQuantity;
    }
}