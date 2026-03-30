using MediatR;
using WebAppExam.Application.Customers.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Customers.Queries;

public class GetAllCustomerQuery(string phoneNumber, string customerName) : IRequest<List<CustomerDTO>>
{
    public string PhoneNumber { get; set; } = phoneNumber;
    public string CustomerName { get; set; } = customerName;
}
