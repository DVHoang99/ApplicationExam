using System;
using FluentResults;
using MediatR;
using WebAppExam.Application.Customers.Command;
using WebAppExam.Application.Customers.Services;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Customers.Commands;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<Ulid>>
{
    private readonly ICustomerService _customerService;


    public CreateCustomerCommandHandler(
        ICustomerService customerService
    )
    {
        _customerService = customerService;
    }

    public async Task<Result<Ulid>> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        return await _customerService.CreateCustomerAync(request.CustomerName, request.Email, request.Phone, ct);

    }
}

