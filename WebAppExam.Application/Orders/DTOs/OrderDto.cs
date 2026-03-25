namespace WebAppExam.Application.Orders.DTOs;

public class OrderDto
{
    public Ulid Id { get; set; }

    public Ulid CustomerId { get; set; }

    public string Status { get; set; }

    public decimal TotalAmount { get; set; }

    public List<OrderDetailDto> Details { get; set; } = new();
}