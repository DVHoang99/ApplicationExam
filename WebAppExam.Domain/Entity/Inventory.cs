using System;

namespace WebAppExam.Domain.Entity;

public class Inventory : EntityBase
{
    public Ulid ProductId { get; set; }
    public string Name { get; set; }
    public int Stock { get; set; }

    protected Inventory()
    {
    }
    internal void UpdateStock(int additionalStock)
    {
        Stock += additionalStock;
    }

    internal Inventory(Ulid productId, int stock, string name)
    {
        Id = Ulid.NewUlid();
        ProductId = productId;
        Stock = stock;
        CreatedAt = DateTime.UtcNow;
        Name = name;
    }

    internal void DeleteInventory()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
