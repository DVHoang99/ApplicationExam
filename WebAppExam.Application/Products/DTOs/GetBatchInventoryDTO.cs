using System;
using System.Text.Json.Serialization;

namespace WebAppExam.Application.Products.DTOs;

public class GetBatchInventoryDTO
{
    [JsonPropertyName("productId")]
    public string ProductId { get; set; }
    [JsonPropertyName("stockQuantity")]
    public int StockQuantity { get; set; }
    [JsonPropertyName("correlationId")]

    public string CorrelationId { get; set; }
    [JsonPropertyName("wareHouseId")]
    public string WareHouseId { get; set; }
    [JsonPropertyName("wareHouse")]
    public WareHouseDTO WareHouse { get; set; }

}
