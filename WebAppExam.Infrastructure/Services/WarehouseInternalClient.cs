using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Services;

namespace WebAppExam.Infrastructure.Services;

public class WarehouseInternalClient : IWareHouseService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public WarehouseInternalClient(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }
    public async Task<WareHouseDTO?> GetWareHouseAsync(string WareHouseId, CancellationToken cancellationToken = default)
    {
        //var secretKey = _config["InternalSettings:WarehouseSecretKey"];

        var path = $"api/warehouses/{WareHouseId}"; // Kiểm tra lại plural (warehouse hay warehouses)

        var response = await _httpClient.GetAsync(path, cancellationToken);
        // var request = new HttpRequestMessage(HttpMethod.Get, $"api/warehouse/{WareHouseId}");
        // //request.Headers.Add("X-Internal-Key", secretKey);

        // var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(jsonString);
            return node?["data"].Deserialize<WareHouseDTO>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true 
            });
        }

        return null;
    }
}
