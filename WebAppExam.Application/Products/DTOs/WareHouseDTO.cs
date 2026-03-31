using System;
using System.Text.Json.Serialization;

namespace WebAppExam.Application.Products.DTOs;

public class WareHouseDTO
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("adress")]
    public string Address { get; set; }
    [JsonPropertyName("owerName")]
    public string OwerName { get; set; }
    [JsonPropertyName("owerEmail")]
    public string OwerEmail { get; set; }
    [JsonPropertyName("owerPhone")]
    public string OwerPhone { get; set; }
}
