using System;
using FluentResults;
using MediatR;
using WebAppExam.Application.Customers.DTOs;
using WebAppExam.Application.Customers.Services;

namespace WebAppExam.Application.Customers.Queries;

public class GetAllCustomerQueryHandler : IRequestHandler<GetAllCustomerQuery, Result<List<CustomerDTO>>>
{
    private readonly ICustomerService _customerService;

    public GetAllCustomerQueryHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Result<List<CustomerDTO>>> Handle(GetAllCustomerQuery request, CancellationToken ct)
    {
        return await _customerService.GetAllCustomersAsync(request.PhoneNumber, request.CustomerName, request.PageNumber, request.PageSize, ct);
    }
}