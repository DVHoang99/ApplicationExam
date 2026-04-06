using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Customers.Commands;

public class UpdateCustomerCommand(Ulid id, string customerName, string email, string phone) : ICommand<Ulid>
{
    public Ulid Id { get; set; } = id;
    public string CustomerName { get; set; } = customerName;
    public string Email { get; set; } = email;
    public string Phone { get; set; } = phone;

    public static UpdateCustomerCommand Init(Ulid id, string customerName, string email, string phone)
    {
        return new UpdateCustomerCommand(id, customerName, email, phone);
    }
}


