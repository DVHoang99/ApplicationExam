
using System.Text.Json.Serialization;

namespace WebAppExam.Application.Products.DTOs;

public class ProductDTO
{
    public Ulid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public string WareHouseId { get; set; }
    public int Stock { get; set; }
    public WareHouseDTO WareHouse { get; set; }
    
    // Required for deserialization
    public ProductDTO()
    {
    }

    [JsonConstructor]
    public ProductDTO(Ulid id, string name, string description, int price, string wareHouseId, int stock, WareHouseDTO wareHouse)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        WareHouseId = wareHouseId;
        Stock = stock;
        WareHouse = wareHouse;
    }

    public static ProductDTO Init(Ulid id, string name, string description, int price, string wareHouseId, int stock, WareHouseDTO? wareHouse)
    {
        if (wareHouse == null)
        {
            wareHouse = new WareHouseDTO();
        }
        
        return new ProductDTO(id, name, description, price, wareHouseId, stock, wareHouse);
    }
}
