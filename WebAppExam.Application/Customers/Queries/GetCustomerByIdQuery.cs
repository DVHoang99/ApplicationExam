using System;
using MediatR;
using WebAppExam.Domain;

namespace WebAppExam.Application.Customers.Queries;

public class GetCustomerByIdQuery(Ulid id) : IRequest<Customer>
{
    public Ulid id { get; set; } = id;
}
