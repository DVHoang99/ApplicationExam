using System.Text.Json.Serialization;
using WebAppExam.Domain;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Application.Orders.DTOs;

public class OrderDTO
{
    public Ulid Id { get; init; }

    public Ulid CustomerId { get; init; }

    public OrderStatus Status { get; init; }

    public decimal TotalAmount { get; init; }
    public string Address { get; init; }
    public string CustomerName { get; init; }
    public string PhoneNumber { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<OrderDetailDTO> Details { get; init; }

    /// <summary>
    /// Parameterless constructor required for JSON deserialization.
    /// Marked as private to prevent direct instantiation via the 'new' keyword from external code,
    /// enforcing the use of the static <see cref="Create"/> factory method.
    /// The [JsonConstructor] attribute grants the ASP.NET Core framework permission to bypass the private access modifier.
    /// </summary>
    /// 
    [JsonConstructor]
    private OrderDTO() { }


    private OrderDTO(Ulid id, Ulid customerId, OrderStatus status, decimal totalAmount, string address, string customerName, string phoneNumber, DateTime createdAt, List<OrderDetailDTO> details)
    {
        Id = id;
        CustomerId = customerId;
        Status = status;
        TotalAmount = totalAmount;
        Address = address;
        CustomerName = customerName;
        PhoneNumber = phoneNumber;
        CreatedAt = createdAt;
        Details = details;
    }

    public static OrderDTO Init(Ulid id, Ulid customerId, OrderStatus status, decimal totalAmount, string address, string customerName, string phoneNumber, DateTime createdAt, List<OrderDetail> details)
    {
        var orderDetailDTO = details.Select(x =>
        OrderDetailDTO.FromResult(
            x.ProductId,
            x.Quantity,
            x.Price,
            x.WareHouseId.ToString()))
            .ToList();

        return new OrderDTO(id, customerId, status, totalAmount, address, customerName, phoneNumber, createdAt, orderDetailDTO);
    }
}