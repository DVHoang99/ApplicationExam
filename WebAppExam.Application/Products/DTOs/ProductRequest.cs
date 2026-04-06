using System;

namespace WebAppExam.Application.Products.DTOs;

public class ProductRequestDTO
{
    public Ulid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Price { get; set; }
    public required string WareHouseId { get; set; }
    public int Stock { get; set; }
    public string? CorreclationId { get; set; }
}
