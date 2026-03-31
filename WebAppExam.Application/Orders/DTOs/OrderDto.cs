using WebAppExam.Domain.Enum;

namespace WebAppExam.Application.Orders.DTOs;

public class OrderDto
{
    public Ulid Id { get; set; }

    public Ulid CustomerId { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }
    public string Address { get; set; }
    public string CustomerName { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderDetailDto> Details { get; set; } = new();
}