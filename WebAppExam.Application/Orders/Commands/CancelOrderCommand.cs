using System;
using System.Windows.Input;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Orders.Commands;

public class CancelOrderCommand(Ulid id) : ICommand<Ulid>
{
    public Ulid Id { get; set; } = id;
}
