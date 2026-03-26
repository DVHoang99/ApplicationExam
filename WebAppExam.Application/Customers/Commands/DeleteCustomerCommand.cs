using System;
using MediatR;
using WebAppExam.Infrastructure.UnitOfWork;

namespace WebAppExam.Application.Customers.Commands;

public class DeleteCustomerCommand(Ulid id) : IRequest<Ulid>
{
    public Ulid Id { get; set; } = id;
}

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Ulid>
{
    private readonly IUnitOfWork _uow;

    public DeleteCustomerCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Ulid> Handle(DeleteCustomerCommand request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.Id, ct);

        if (customer == null)
            throw new Exception("Customer not found");

        customer.DeletedAt = DateTime.Now;

        _uow.Customers.Update(customer);
        await _uow.SaveChangesAsync(ct);
        return customer.Id;
    }
}
