using System;

namespace WebAppExam.Domain.Entity;

public class Inventory : EntityBase
{
    public Ulid ProductId { get; set; }
    public int Stock { get; set; }

    protected Inventory()
    {
    }
    internal void UpdateStock(int additionalStock)
    {
        Stock += additionalStock;
    }

    internal Inventory(Ulid productId, int price, int stock)
    {
        if (price <= 0) throw new ArgumentException("Price greater than 0 is required");

        Id = Ulid.NewUlid();
        ProductId = productId;
        Stock = stock;
        CreatedAt = DateTime.UtcNow;
    }

    internal void DeleteInventory()
    {
        DeletedAt = DateTime.UtcNow;
    }
}
