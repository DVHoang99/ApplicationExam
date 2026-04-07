using System;
using System.Text.Json.Serialization;

namespace WebAppExam.Application.Products.DTOs;

public class WareHouseDTO
{
    [JsonPropertyName("id")]
    public string Id { get; private set; }
    [JsonPropertyName("address")]
    public string Address { get; private set; }
    [JsonPropertyName("ownerName")]
    public string OwerName { get; private set; }
    [JsonPropertyName("ownerEmail")]
    public string OwerEmail { get; private set; }
    [JsonPropertyName("ownerPhone")]
    public string OwerPhone { get; private set; }

    private WareHouseDTO(string id, string address, string ownerName, string ownerEmail, string ownerPhone)
    {
        Id = id;
        Address = address;
        OwerName = ownerName;
        OwerEmail = ownerEmail;
        OwerPhone = ownerPhone;
    }

    public WareHouseDTO()
    {
        Id = "";
        Address = "";
        OwerName = "";
        OwerEmail = "";
        OwerPhone = "";
    }

    public static WareHouseDTO Init(string id, string address, string ownerName, string ownerEmail, string ownerPhone)
    {
        return new WareHouseDTO(id, address, ownerName, ownerEmail, ownerPhone);
    }
}
