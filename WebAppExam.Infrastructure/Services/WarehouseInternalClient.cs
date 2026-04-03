using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Services;

namespace WebAppExam.Infrastructure.Services;

public class WarehouseInternalClient : IWareHouseService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WarehouseInternalClient(IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _config = config;
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<WareHouseDTO?> GetWareHouseAsync(string WareHouseId, CancellationToken cancellationToken = default)
    {
        //var secretKey = _config["InternalSettings:WarehouseSecretKey"];

        var path = $"api/warehouses/{WareHouseId}";
        var request = new HttpRequestMessage(HttpMethod.Get, path);

        var context = _httpContextAccessor.HttpContext;
        if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            request.Headers.Add("Authorization", authHeader.ToString());
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);

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
