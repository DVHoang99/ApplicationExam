using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Application.Customers.DTOs;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.UnitOfWork;

namespace WebAppExam.Application.Customers.Queries;

public class GetAllCustomerQuery(string phoneNumber, string customerName) : IRequest<List<CustomerDTO>>
{
    public string PhoneNumber { get; set; } = phoneNumber;
    public string CustomerName { get; set; } = customerName;
}

public class GetAllCustomerQueryHandler : IRequestHandler<GetAllCustomerQuery, List<CustomerDTO>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetAllCustomerQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }


    public async Task<List<CustomerDTO>> Handle(GetAllCustomerQuery request, CancellationToken ct)
    {
        var query = _customerRepository.Query().Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.CustomerName))
        {
            query = query.Where(x => EF.Functions.Like(x.CustomerName, $"%{request.CustomerName}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            query = query.Where(x => EF.Functions.Like(x.PhoneNumber, $"%{request.PhoneNumber}%"));
        }

        var customers = await query.ToListAsync(ct);

        return customers.Select(x => new CustomerDTO
        {
            Id = x.Id,
            CustomerName = x.CustomerName,
            Email = x.Email,
            Phone = x.PhoneNumber
        }).ToList();
    }
}
