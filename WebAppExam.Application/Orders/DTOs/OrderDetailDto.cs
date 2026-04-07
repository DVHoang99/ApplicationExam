using System.ComponentModel;

namespace WebAppExam.Application.Orders.DTOs;

public class OrderDetailDTO
{
    public Ulid ProductId { get; private set; }

    public int Quantity { get; private set; }

    public int Price { get; private set; }
    public string WareHouseId { get; private set; }

    public decimal SubTotal => Quantity * Price;

    private OrderDetailDTO(Ulid productId, int quantity, int price, string wareHouseId)
    {
        ProductId = productId;
        Quantity = quantity;
        Price = price;
        WareHouseId = wareHouseId;
    }

    public static OrderDetailDTO FromResult(Ulid productId, int quantity, int price, string wareHouseId)
    {
        return new OrderDetailDTO(productId, quantity, price, wareHouseId);
    }
}