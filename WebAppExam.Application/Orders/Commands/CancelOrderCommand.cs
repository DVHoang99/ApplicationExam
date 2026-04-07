using System;
using System.Windows.Input;
using FluentResults;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Orders.Commands;

public class CancelOrderCommand(Ulid id) : ICommand<Result<Ulid>>
{
    public Ulid Id { get; private set; } = id;
    public static CancelOrderCommand Init(Ulid id)
    {
        return new CancelOrderCommand(id);
    }
}
