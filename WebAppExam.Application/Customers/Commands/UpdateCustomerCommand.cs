using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Customers.Commands;

public class UpdateCustomerCommand(Ulid id) : ICommand<Ulid>
{
    public Ulid Id { get; set; } = id;
    public string CustomerName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}


