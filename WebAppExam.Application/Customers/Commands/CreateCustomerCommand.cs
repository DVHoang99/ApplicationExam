using System;
using MediatR;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.UnitOfWork;

namespace WebAppExam.Application.Customers.Command;

public class CreateCustomerCommand : IRequest<Ulid>
{
    public string CustomerName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Ulid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _uow;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork uow)
    {
        _uow = uow;
        _customerRepository = customerRepository;
    }

    public async Task<Ulid> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        var customerByEmail = await _uow.Customers.GetCustomerByEmailAsync(request.Email);

        if (customerByEmail != null)
            throw new Exception("Customer already exists");

        var customer = new Customer
        {
            Id = Ulid.NewUlid(),
            CustomerName = request.CustomerName,
            Email = request.Email,
            PhoneNumber = request.Phone
        };

        await _uow.Customers.AddAsync(customer, ct);

        await _uow.SaveChangesAsync(ct);
        return customer.Id;
    }
}
