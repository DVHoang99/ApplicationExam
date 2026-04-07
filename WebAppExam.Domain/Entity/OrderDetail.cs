namespace WebAppExam.Domain;

public class OrderDetail : EntityBase
{
    public Ulid OrderId { get; private set; }
    public Ulid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public int Price { get; private set; }
    public Ulid WareHouseId { get; private set; }
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