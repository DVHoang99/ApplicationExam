using System;
using System.Text.Json.Serialization;

namespace WebAppExam.Application.Products.DTOs;

public class GetBatchInventoryDTO
{
    [JsonPropertyName("productId")]
    public string ProductId { get; init; }
    [JsonPropertyName("stockQuantity")]
    public int StockQuantity { get; init; }
    [JsonPropertyName("correlationId")]

    public string CorrelationId { get; init; }
    [JsonPropertyName("wareHouseId")]
    public string WareHouseId { get; init; }
    [JsonPropertyName("wareHouse")]
    public WareHouseDTO? WareHouse { get; init; }

    private GetBatchInventoryDTO(string productId, int stockQuantity, string correlationId, string wareHouseId, WareHouseDTO? wareHouse)
    {
        ProductId = productId;
        StockQuantity = stockQuantity;
        CorrelationId = correlationId;
        WareHouseId = wareHouseId;
        WareHouse = wareHouse;
    }
    public static GetBatchInventoryDTO Init(string productId, int stockQuantity, string correlationId, string wareHouseId, WareHouseDTO? wareHouse)
    {
        return new GetBatchInventoryDTO(productId, stockQuantity, correlationId, wareHouseId, wareHouse);
    }
}
