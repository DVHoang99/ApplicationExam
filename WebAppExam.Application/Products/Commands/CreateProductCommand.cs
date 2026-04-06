using System;
using System.Windows.Input;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Products.Commands;

public class CreateProductCommand : ICommand<Ulid>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Price { get; set; }
    public required string WareHouseId { get; set; }
    public int Stock { get; set; }

    public static CreateProductCommand Init(string name, string? description, int price, string wareHouseId, int stock)
    {
        return new CreateProductCommand
        {
            Name = name,
            Description = description,
            Price = price,
            WareHouseId = wareHouseId,
            Stock = stock
        };
    }
}
