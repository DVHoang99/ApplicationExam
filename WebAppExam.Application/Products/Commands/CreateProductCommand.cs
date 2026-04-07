using System;
using System.Windows.Input;
using FluentResults;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Products.Commands;

public class CreateProductCommand(string name, string? description, int price, string wareHouseId, int stock) : ICommand<Result<Ulid>>
{
    public string Name { get; private set; } = name;
    public string? Description { get; private set; } = description;
    public int Price { get; private set; } = price;
    public string WareHouseId { get; private set; } = wareHouseId;
    public int Stock { get; private set; } = stock;

    public static CreateProductCommand Init(string name, string? description, int price, string wareHouseId, int stock)
    {
        return new CreateProductCommand(name, description, price, wareHouseId, stock);
    }
}
