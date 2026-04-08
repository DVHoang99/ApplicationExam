
using System.Text.Json;
using System.Text.Json.Nodes;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Services;
using WebAppExam.Infrastructure.Protos;

namespace WebAppExam.Infrastructure.Services;

public class WarehouseInternalClient : IWareHouseService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly WarehouseGrpc.WarehouseGrpcClient _warehouseClient;

    public WarehouseInternalClient(
        IConfiguration config,
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        WarehouseGrpc.WarehouseGrpcClient warehouseClient)
    {
        _config = config;
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _warehouseClient = warehouseClient;
    }
    public async Task<Application.Products.DTOs.WareHouseDTO?> GetWareHouseAsync(string WareHouseId, CancellationToken cancellationToken = default)
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
            return node?["data"].Deserialize<Application.Products.DTOs.WareHouseDTO>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return null;
    }

    public async Task<Application.Products.DTOs.WareHouseDTO?> GetWareHouseGrpcAsync(string wareHouseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = string.Empty;
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                token = authHeader.ToString();
            }

            var headers = new Metadata
            {
                { "Authorization", $"{token}" }
            };

            var request = new GetWarehouseRequest
            {
                Id = wareHouseId
            };

            var response = await _warehouseClient.GetWarehouseAsync(request, headers: headers, cancellationToken: cancellationToken);

            return Application.Products.DTOs.WareHouseDTO.Init(response.Id, response.Address, response.OwnerName, response.OwnerEmail, response.OwnerPhone); ;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
        catch (RpcException ex)
        {
            throw new Exception($"gRPC call failed. Error code: {ex.StatusCode}. Details: {ex.Status.Detail}");
        }
    }
}
