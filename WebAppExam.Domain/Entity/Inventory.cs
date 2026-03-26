using System;

namespace WebAppExam.Domain.Entity;

public class Inventory : EntityBase
{
    public Ulid ProductId { get; set; }
    public int Price { get; set; }
    public int Stock { get; set; }

    internal void UpdateStock(int additionalStock)
    {
        Stock += additionalStock;
    }

    internal void UpdatePrice(int newPrice)
    {
        Price = newPrice;
    }

    internal Inventory(Ulid productId, int price, int stock)
    {
        if (price <= 0) throw new ArgumentException("Price greater than 0 is required");

        Id = Ulid.NewUlid();
        ProductId = productId;
        Price = price;
        Stock = stock;
    }


}
