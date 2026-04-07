using System;

namespace WebAppExam.Application.Products.DTOs;

public class ProductRequestDTO
{
    public Ulid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public int Price { get; init; }
    public required string WareHouseId { get; init; }
    public int Stock { get; init; }
    public string? CorreclationId { get; init; }
}
