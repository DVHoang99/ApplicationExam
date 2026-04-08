using System;
using System.Text.Json.Serialization;

namespace WebAppExam.Application.Products.DTOs;

public class WareHouseDTO
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    [JsonPropertyName("address")]
    public string Address { get; init; }
    [JsonPropertyName("ownerName")]
    public string OwnerName { get; init; }
    [JsonPropertyName("ownerEmail")]
    public string OwnerEmail { get; init; }
    [JsonPropertyName("ownerPhone")]
    public string OwnerPhone { get; init; }

    private WareHouseDTO(string id, string address, string ownerName, string ownerEmail, string ownerPhone)
    {
        Id = id;
        Address = address;
        OwnerName = ownerName;
        OwnerEmail = ownerEmail;
        OwnerPhone = ownerPhone;
    }

    public WareHouseDTO()
    {
        Id = "";
        Address = "";
        OwnerName = "";
        OwnerEmail = "";
        OwnerPhone = "";
    }

    public static WareHouseDTO Init(string id, string address, string ownerName, string ownerEmail, string ownerPhone)
    {
        return new WareHouseDTO(id, address, ownerName, ownerEmail, ownerPhone);
    }
}
