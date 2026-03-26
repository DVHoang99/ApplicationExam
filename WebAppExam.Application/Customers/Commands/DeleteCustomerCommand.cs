using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Customers.Commands;

public class DeleteCustomerCommand(Ulid id) : ICommand<Ulid>
{
    public Ulid Id { get; set; } = id;
}