using FluentResults;
using MediatR;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Customers.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Customers.Commands;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<Ulid>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICacheService _cacheService;
    private readonly ICustomerService _customerService;



    public DeleteCustomerCommandHandler(ICustomerRepository customerRepository, ICacheService cacheService, ICustomerService customerService)
    {
        _cacheService = cacheService;
        _customerRepository = customerRepository;
        _customerService = customerService;
    }

    public async Task<Result<Ulid>> Handle(DeleteCustomerCommand request, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, ct);

        if (customer == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Customer", "Customer not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        customer.DeletedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;

        _customerRepository.Update(customer);

        await _cacheService.RemoveByPrefixAsync($"customer_detail:{request.Id}");
        return customer.Id;
    }
}

