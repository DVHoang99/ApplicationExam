using System;
using System.Text.Json.Serialization;

namespace WebAppExam.Application.Products.DTOs;

public class WareHouseDTO
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("address")]
    public string Address { get; set; }
    [JsonPropertyName("ownerName")]
    public string OwerName { get; set; }
    [JsonPropertyName("ownerEmail")]
    public string OwerEmail { get; set; }
    [JsonPropertyName("ownerPhone")]
    public string OwerPhone { get; set; }
}
