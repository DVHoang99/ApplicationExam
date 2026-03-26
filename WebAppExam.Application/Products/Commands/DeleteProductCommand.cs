using System;
using System.Windows.Input;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Products.Commands;

public class DeleteProductCommand(Ulid id) : ICommand<Ulid>
{
    public Ulid ProductId { get; set; } = id;
}
