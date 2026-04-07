using System;

namespace WebAppExam.Application.Products.DTOs;

public class InventoryDTO
{
    public string CorrelationId { get; private set; }
    public Ulid Id { get; private set; }
    public string Name { get; private set; }
    public int Stock { get; private set; }
    public string WareHouseId { get; private set; }
    public WareHouseDTO wareHouseDTO { get; private set; }
}
