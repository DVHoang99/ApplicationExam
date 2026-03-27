using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Customers.Command;

public class CreateCustomerCommand : ICommand<Ulid>
{
    public string CustomerName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}