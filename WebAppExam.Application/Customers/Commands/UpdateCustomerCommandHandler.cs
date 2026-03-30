using System;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Customers.Commands;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Ulid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICacheService _cacheService;

    public UpdateCustomerCommandHandler(ICustomerRepository customerRepository, ICacheService cacheService)
    {
        _customerRepository = customerRepository;
        _cacheService = cacheService;
    }
    public async Task<Ulid> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, ct);

        if (customer == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Customer", "Customer not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        customer.CustomerName = request.CustomerName;
        customer.Email = request.Email;
        customer.PhoneNumber = request.Phone;
        _customerRepository.Update(customer);
        await _cacheService.RemoveByPrefixAsync($"customer_detail:{request.Id}");
        return customer.Id;
    }
}
