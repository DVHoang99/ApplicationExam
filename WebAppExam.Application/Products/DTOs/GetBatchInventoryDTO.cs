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
    public WareHouseDTO WareHouse { get; init; }
}
