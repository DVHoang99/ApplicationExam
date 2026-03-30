using System;
using MediatR;
using WebAppExam.Application.Customers.Command;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Customers.Commands;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Ulid>
{
    private readonly ICustomerRepository _customerRepository;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Ulid> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        var customerByEmail = await _customerRepository.GetCustomerByEmailAsync(request.Email);

        if (customerByEmail != null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Customer", "Customer already exists");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        var customer = new Customer
        {
            Id = Ulid.NewUlid(),
            CustomerName = request.CustomerName,
            Email = request.Email,
            PhoneNumber = request.Phone,
            CreatedAt = DateTime.UtcNow,
        };

        await _customerRepository.AddAsync(customer, ct);
        return customer.Id;
    }
}

