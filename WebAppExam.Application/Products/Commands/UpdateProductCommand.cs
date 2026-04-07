using System;
using System.Windows.Input;
using FluentResults;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Products.Commands;

public class UpdateProductCommand(Ulid id, string name, string? description, int price, string wareHouseId, int stock) : ICommand<Result<Ulid>>
{
    public Ulid ProductId { get; private set; } = id;
    public string Name { get; private set; } = name;
    public string? Description { get; private set; } = description;
    public int Price { get; private set; } = price;
    public string WareHouseId { get; private set; } = wareHouseId;
    public int Stock { get; private set; } = stock;

    public static UpdateProductCommand Init(Ulid id, ProductRequestDTO input)
    {
        return new UpdateProductCommand(id, input.Name, input.Description, input.Price, input.WareHouseId, input.Stock);
    }
}
