using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppExam.Application.Products.DTOs;

public class ProductDTO
{
    public Ulid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public string WareHouseId { get; set; }
    public int Stock { get; set; }
    public WareHouseDTO WareHouse { get; set; } = new();

    //public List<InventoryDTO> Inventories { get; set; } = new();
}
