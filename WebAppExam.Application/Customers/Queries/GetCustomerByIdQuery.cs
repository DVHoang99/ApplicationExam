using System;
using FluentResults;
using MediatR;
using WebAppExam.Application.Customers.DTOs;
using WebAppExam.Domain;

namespace WebAppExam.Application.Customers.Queries;

public class GetCustomerByIdQuery(Ulid id) : IRequest<Result<CustomerDTO>>
{
    public Ulid Id { get; private set; } = id;
    public static GetCustomerByIdQuery Init(Ulid id)
    {
        return new GetCustomerByIdQuery(id);
    }
}
