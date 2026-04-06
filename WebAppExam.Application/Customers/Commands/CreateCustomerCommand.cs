using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Customers.Command;

public class CreateCustomerCommand(string customerName, string email, string phone) : ICommand<Ulid>
{
    public string CustomerName { get; set; } = customerName;
    public string Email { get; set; } = email;
    public string Phone { get; set; } = phone;

    public static CreateCustomerCommand Create(string customerName, string email, string phone)
    {
        return new CreateCustomerCommand(customerName, email, phone);
    }
}