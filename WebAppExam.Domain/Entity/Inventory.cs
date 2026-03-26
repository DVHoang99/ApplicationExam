using System;

namespace WebAppExam.Domain.Entity;

public class Inventory : EntityBase
{
    public Ulid ProductId { get; set; }
    public int Price { get; set; }
    public int Stock { get; set; }
    public Product Product { get; set; }
}
