using System;

namespace WebAppExam.Application.Customers.DTOs;

public class CustomerDTO
{
    public Ulid Id { get; set; }
    public string CustomerName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}
