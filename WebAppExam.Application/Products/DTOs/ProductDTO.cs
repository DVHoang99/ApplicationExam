using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppExam.Application.Products.DTOs;

public class ProductDTO
{
    public Ulid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Price { get; private set; }
    public string WareHouseId { get; private set; }
    public int Stock { get; private set; }
    public WareHouseDTO WareHouse { get; private set; }
    private ProductDTO(Ulid id, string name, string description, int price, string wareHouseId, int stock, WareHouseDTO wareHouse)
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
