using FluentResults;
using MediatR;
using WebAppExam.Application.Customers.DTOs;
using WebAppExam.Application.Customers.Services;

namespace WebAppExam.Application.Customers.Queries;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDTO>>
{
    private readonly ICustomerService _customerService;

    public GetCustomerByIdQueryHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    public async Task<Result<CustomerDTO>> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        return await _customerService.GetCustomerByIdAsync(request.Id, ct);
    }
}
