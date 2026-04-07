using System;
using System.Windows.Input;
using FluentResults;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Products.Commands;

public class DeleteProductCommand(Ulid id, string wareHouseId) : ICommand<Result<Ulid>>
{
    public Ulid ProductId { get; private set; } = id;
    public string WareHouseId { get; private set; } = wareHouseId;

    public static DeleteProductCommand Init(Ulid id, string wareHouseId)
    {
        return new DeleteProductCommand(id, wareHouseId);
    }
}
