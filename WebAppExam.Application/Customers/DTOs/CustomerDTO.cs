using System;
using System.Text.Json.Serialization;

namespace WebAppExam.Application.Customers.DTOs;

public class CustomerDTO
{
    public Ulid Id { get; init; }
    public string CustomerName { get; init; }
    public string Email { get; init; }
    public string Phone { get; init; }

    /// <summary>
    /// Parameterless constructor required for JSON deserialization.
    /// Marked as private to prevent direct instantiation via the 'new' keyword from external code,
    /// enforcing the use of the static <see cref="Create"/> factory method.
    /// The [JsonConstructor] attribute grants the ASP.NET Core framework permission to bypass the private access modifier.
    /// </summary>
    /// 
    [JsonConstructor]
    private CustomerDTO() { }

    private CustomerDTO(Ulid id, string customerName, string email, string phone)
    {
        Id = id;
        CustomerName = customerName;
        Email = email;
        Phone = phone;
    }

    public static CustomerDTO FromResult(Ulid id, string customerName, string email, string phone)
    {
        return new CustomerDTO(id, customerName, email, phone);
    }
}
