using System;
using System.Text.Json.Serialization;

namespace WebAppExam.Application.Products.DTOs;

public class GetBatchInventoryDTO
{
    [JsonPropertyName("productId")]
    public string ProductId { get; private set; }
    [JsonPropertyName("stockQuantity")]
    public int StockQuantity { get; private set; }
    [JsonPropertyName("correlationId")]

    public string CorrelationId { get; private set; }
    [JsonPropertyName("wareHouseId")]
    public string WareHouseId { get; private set; }
    [JsonPropertyName("wareHouse")]
    public WareHouseDTO WareHouse { get; private set; }

}
