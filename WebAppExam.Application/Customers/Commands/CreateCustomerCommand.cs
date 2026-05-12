using FluentResults;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Customers.Commands;

public class CreateCustomerCommand(string customerName, string email, string phone) : ICommand<Result<Ulid>>
{
    public string CustomerName { get; private set; } = customerName;
    public string Email { get; private set; } = email;
    public string Phone { get; private set; } = phone;

    public static CreateCustomerCommand Create(string customerName, string email, string phone)
    {
        return new CreateCustomerCommand(customerName, email, phone);
    }
}