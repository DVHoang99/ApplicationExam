using System;
using MediatR;
using WebAppExam.Infrastructure.UnitOfWork;

namespace WebAppExam.Application.Customers.Commands;

public class UpdateCustomerCommand(Ulid id) : IRequest<Ulid>
{
    public Ulid Id { get; set; } = id;
    public string CustomerName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Ulid>
{
    private readonly IUnitOfWork _uow;

    public UpdateCustomerCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }
    public async Task<Ulid> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.Id, ct);

        if (customer == null)
            throw new Exception("Customer not found");

        customer.CustomerName = request.CustomerName;
        customer.Email = request.Email;
        customer.PhoneNumber = request.Phone;
        _uow.Customers.Update(customer);
        await _uow.SaveChangesAsync(ct);
        return customer.Id;
    }
}

