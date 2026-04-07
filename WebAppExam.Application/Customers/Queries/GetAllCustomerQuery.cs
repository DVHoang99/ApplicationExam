using FluentResults;
using MediatR;
using WebAppExam.Application.Customers.DTOs;

namespace WebAppExam.Application.Customers.Queries;

public class GetAllCustomerQuery(string phoneNumber, string customerName, int pageNumber, int pageSize) : IRequest<Result<List<CustomerDTO>>>
{
    public string PhoneNumber { get; private set; } = phoneNumber;
    public string CustomerName { get; private set; } = customerName;
    public int PageNumber { get; private set; } = pageNumber;
    public int PageSize { get; private set; } = pageSize;

    public static GetAllCustomerQuery GetAll(string phoneNumber, string customerName, int pageNumber, int pageSize)
    {
        return new GetAllCustomerQuery(phoneNumber, customerName, pageNumber, pageSize);
    }
}
