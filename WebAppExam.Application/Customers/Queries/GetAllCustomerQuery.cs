using MediatR;
using WebAppExam.Application.Customers.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Customers.Queries;

public class GetAllCustomerQuery(string phoneNumber, string customerName, int pageNumber, int pageSize) : IRequest<List<CustomerDTO>>
{
    public string PhoneNumber { get; set; } = phoneNumber;
    public string CustomerName { get; set; } = customerName;
    public int PageNumber { get; set; } = pageNumber;
    public int PageSize { get; set; } = pageSize;
}
