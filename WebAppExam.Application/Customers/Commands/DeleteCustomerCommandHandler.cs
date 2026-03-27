using MediatR;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Customers.Commands;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Ulid>
{
    private readonly ICustomerRepository _customerRepository;

    public DeleteCustomerCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }
    public async Task<Ulid> Handle(DeleteCustomerCommand request, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, ct);

        if (customer == null)
            throw new Exception("Customer not found");

        customer.DeletedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;

        _customerRepository.Update(customer);
        return customer.Id;
    }
}

