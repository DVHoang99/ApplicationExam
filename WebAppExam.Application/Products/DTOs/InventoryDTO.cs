using System;

namespace WebAppExam.Application.Products.DTOs;

public class InventoryDTO
{
    public Ulid Id { get; set; }
    public string Name { get; set; }
    public int Stock { get; set; }
    public string WareHouseId { get; set; }
}
