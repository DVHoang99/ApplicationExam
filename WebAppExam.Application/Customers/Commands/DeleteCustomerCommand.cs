using FluentResults;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Customers.Commands;

public class DeleteCustomerCommand(Ulid id) : ICommand<Result<Ulid>>
{
    public Ulid Id { get; private set; } = id;
    public static DeleteCustomerCommand Init(Ulid id)
    {
        return new DeleteCustomerCommand(id);
    }
}