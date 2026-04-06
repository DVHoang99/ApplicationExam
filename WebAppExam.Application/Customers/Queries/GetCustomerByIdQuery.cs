using System;
using MediatR;
using WebAppExam.Application.Customers.DTOs;
using WebAppExam.Domain;

namespace WebAppExam.Application.Customers.Queries;

public class GetCustomerByIdQuery(Ulid id) : IRequest<CustomerDTO>
{
    public Ulid Id { get; set; } = id;
}
