using FluentResults;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Customers.Commands;

public class UpdateCustomerCommand(Ulid id, string customerName, string email, string phone) : ICommand<Result<Ulid>>
{
    public Ulid Id { get; private set; } = id;
    public string CustomerName { get; private set; } = customerName;
    public string Email { get; private set; } = email;
    public string Phone { get; private set; } = phone;

    public static UpdateCustomerCommand Init(Ulid id, string customerName, string email, string phone)
    {
        return new UpdateCustomerCommand(id, customerName, email, phone);
    }
}


