using MediatR;
using WebAppExam.Infra;

namespace WebAppExam.Application.Customer.Commands
{
    public class CreateCustomerCommand(string customerName,
    string phoneNumber,
    string email) : IRequest<Guid>
    {
        public string CustomerName { get; } = customerName;
        public string PhoneNumber { get; } = phoneNumber;
        public string Email { get; } = email;
    }

    public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Guid>
    {
        private readonly ApplicationDbContext _context;

        public CreateCustomerHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = new Domain.Customer
            {
                Id = Guid.NewGuid(),
                CustomerName = request.CustomerName,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

            return customer.Id;
        }
    }
}
