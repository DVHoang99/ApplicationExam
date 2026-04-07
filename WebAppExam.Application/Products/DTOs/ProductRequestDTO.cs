using System;

namespace WebAppExam.Application.Products.DTOs;

public class ProductRequest
{
    public Ulid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Price { get; private set; }
    public string WareHouseId { get; private set; }
    public int Stock { get; private set; }
}
