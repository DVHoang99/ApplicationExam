using System;

namespace WebAppExam.Application.Products.DTOs;

public class ProductDTO
{
    public Ulid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public List<InventoryDTO> Inventories { get; set; } = new();
}
