using System.ComponentModel;
using System.Text.Json.Serialization;

namespace WebAppExam.Application.Orders.DTOs;

public class OrderDetailDTO
{
    public Ulid ProductId { get; init; }

    public int Quantity { get; init; }

    public int Price { get; init; }
    public string WareHouseId { get; init; }

    public decimal SubTotal => Quantity * Price;

    /// <summary>
    /// Parameterless constructor required for JSON deserialization.
    /// Marked as private to prevent direct instantiation via the 'new' keyword from external code,
    /// enforcing the use of the static <see cref="Create"/> factory method.
    /// The [JsonConstructor] attribute grants the ASP.NET Core framework permission to bypass the private access modifier.
    /// </summary>
    /// 
    [JsonConstructor]
    private OrderDetailDTO() { }


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