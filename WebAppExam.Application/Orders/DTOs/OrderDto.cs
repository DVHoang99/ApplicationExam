using WebAppExam.Domain;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Application.Orders.DTOs;

public class OrderDTO
{
    public Ulid Id { get; private set; }

    public Ulid CustomerId { get; private set; }

    public OrderStatus Status { get; private set; }

    public decimal TotalAmount { get; private set; }
    public string Address { get; private set; }
    public string CustomerName { get; private set; }
    public string PhoneNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<OrderDetailDTO> Details { get; private set; }

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