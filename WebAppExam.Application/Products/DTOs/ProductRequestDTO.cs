using System;

namespace WebAppExam.Application.Products.DTOs;

public class ProductRequest
{
    public Ulid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public string WareHouseId { get; set; }
    public int Stock { get; set; }
}
