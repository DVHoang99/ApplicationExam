using System;
using System.Windows.Input;
using FluentResults;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Orders.Commands;

public class DeleteOrderCommand(Ulid id) : ICommand<Result<Ulid>>
{
    public Ulid Id { get; private set; } = id;
    public static DeleteOrderCommand Init(Ulid id)
    {
        return new DeleteOrderCommand(id);
    }
}
