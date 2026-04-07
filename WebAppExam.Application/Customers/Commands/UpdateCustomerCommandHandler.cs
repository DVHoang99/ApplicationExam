using FluentResults;
using MediatR;
using WebAppExam.Application.Customers.Services;

namespace WebAppExam.Application.Customers.Commands;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<Ulid>>
{
    private readonly ICustomerService _customerService;

    public UpdateCustomerCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    public async Task<Result<Ulid>> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        return await _customerService.UpdateCustomerAsync(request.Id, request.CustomerName, request.Email, request.Phone, ct);
    }
}
