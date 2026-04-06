using System;
using System.Windows.Input;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Orders.Commands;

public class DeleteOrderCommand(Ulid id) : ICommand<Ulid>
{
    public Ulid Id { get; set; } = id;
    public static DeleteOrderCommand Init(Ulid id)
    {
        return new DeleteOrderCommand(id);
    }
}
