using System;
using MediatR;
using WebAppExam.Domain;

namespace WebAppExam.Application.Customers.Queries;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Customer>
{
    
    private readonly IUnitOfWork _uow;

    public GetCustomerByIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Customer> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.id, ct);

        if (customer == null)
            throw new Exception("Customer not found");

        return customer;
    }
}
