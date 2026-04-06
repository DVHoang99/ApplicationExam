using System;
using System.Windows.Input;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Orders.Commands;

public class CancelOrderCommand(Ulid id) : ICommand<Ulid>
{
    public Ulid Id { get; set; } = id;
    public static CancelOrderCommand Init(Ulid id)
    {
        return new CancelOrderCommand(id);
    }
}
