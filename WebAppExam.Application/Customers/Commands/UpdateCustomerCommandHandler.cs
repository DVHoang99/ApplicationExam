using System;
using MediatR;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Customers.Commands;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Ulid>
{
    private readonly ICustomerRepository _customerRepository;

    public UpdateCustomerCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }
    public async Task<Ulid> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, ct);

        if (customer == null)
            throw new Exception("Customer not found");

        customer.CustomerName = request.CustomerName;
        customer.Email = request.Email;
        customer.PhoneNumber = request.Phone;
        _customerRepository.Update(customer);
        return customer.Id;
    }
}
