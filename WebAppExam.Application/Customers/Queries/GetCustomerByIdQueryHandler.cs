using System;
using MediatR;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Customers.DTOs;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Customers.Queries;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDTO>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICacheService _cacheService;


    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository, ICacheService cacheService)
    {
        _cacheService = cacheService;
        _customerRepository = customerRepository;
    }
    public async Task<CustomerDTO> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {

        var customer = await _cacheService.GetAsync($"customer_detail:{request.Id}", async () =>
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id, ct);

            if (customer == null)
            {
                var failure = new FluentValidation.Results.ValidationFailure("Customer", "Customer not found");
                throw new FluentValidation.ValidationException(new[] { failure });
            }
            return new CustomerDTO
            {
                Id = customer.Id,
                CustomerName = customer.CustomerName,
                Email = customer.Email,
                Phone = customer.PhoneNumber
            };
        }, TimeSpan.FromDays(1), ct);


        return customer ?? new CustomerDTO();
    }
}
