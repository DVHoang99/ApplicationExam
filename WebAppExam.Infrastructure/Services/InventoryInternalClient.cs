using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Services;
using WebAppExam.Infrastructure.Protos;

namespace WebAppExam.Infrastructure.Services;

public class InventoryInternalClient : IInventoryService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly InventoryGrpc.InventoryGrpcClient _inventoryGrpcClient;

    public InventoryInternalClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        InventoryGrpc.InventoryGrpcClient inventoryGrpcClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5134/");
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _inventoryGrpcClient = inventoryGrpcClient;
    }

    public async Task<Application.Products.DTOs.InventoryDTO?> CreateInventoryAsync(string wareHouseId, string productId, int stock, string correlationId, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            WareHouseId = wareHouseId,
            ProductId = productId,
            StockQuantity = stock,
            CorrelationId = correlationId
        };

        var response = await SendInternalRequestAsync(HttpMethod.Post, "api/inventories", payload, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var node = JsonNode.Parse(jsonString);
            return node?["data"].Deserialize<Application.Products.DTOs.InventoryDTO>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Error call Service B (Create): {(int)response.StatusCode} - {errorContent}");
    }

    public async Task<List<GetBatchInventoryDTO>> GetInventoryDTOsAsync(List<string> correlationIds, CancellationToken cancellationToken = default)
    {
        var payload = new { CorrelationIds = correlationIds };
        var response = await SendInternalRequestAsync(HttpMethod.Post, "api/inventories/batch", payload, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var node = JsonNode.Parse(jsonString);
            return node?["data"]?.Deserialize<List<GetBatchInventoryDTO>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new List<GetBatchInventoryDTO>();
        }

        return new List<GetBatchInventoryDTO>();
    }

    public async Task CallInventoryToUpdate(string productId, string wareHouseId, int newStock, Guid updateEventId, CancellationToken cancellationToken = default)
    {
        var path = $"api/inventories/{productId}";
        var payload = new { WareHouseId = wareHouseId, Stock = newStock, UpdateEventId = updateEventId };

        var response = await SendInternalRequestAsync(HttpMethod.Put, path, payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Update failed: {response.StatusCode} - {error}");
        }
    }

    public async Task CallInventoryToDelete(string productId, string wareHouseId, CancellationToken cancellationToken = default)
    {
        var path = $"api/inventories/{productId}?wareHouseId={wareHouseId}";
        var response = await SendInternalRequestAsync(HttpMethod.Delete, path, null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Delete failed: {response.StatusCode} - {error}");
        }
    }

    private async Task<HttpResponseMessage> SendInternalRequestAsync(
        HttpMethod method,
        string path,
        object? payload = null,
        CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(method, path);

        var internalKey = _configuration["InternalSettings:ApiKey"];
        if (!string.IsNullOrEmpty(internalKey))
        {
            request.Headers.Add("X-Internal-Key", internalKey);
        }

        var context = _httpContextAccessor.HttpContext;
        if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authHeader.ToString());
        }

        if (payload != null)
        {
            request.Content = JsonContent.Create(payload);
        }

        try
        {
            return await _httpClient.SendAsync(request, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Kết nối đến Service Inventory thất bại: {ex.Message}");
        }
    }

    public async Task<List<GetBatchInventoryDTO>?> GetInventoryDTOsByIdsAsync(List<string> productIds, CancellationToken cancellationToken = default)
    {
        try
        {

            var token = string.Empty;
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                token = authHeader.ToString();
            }

            var request = new GetBatchInventoriesRequest();

            if (productIds != null && productIds.Any())
            {
                request.Ids.AddRange(productIds);
            }

            var headers = new Metadata
            {
                { "Authorization", $"{token}" }
            };

            var response = await _inventoryGrpcClient.GetBatchInventoriesAsync(request, headers: headers, cancellationToken: cancellationToken);

            if (response?.Inventories == null) return null;

            return response.Inventories.Select(x =>
            {
                var warehouseDto = x.WareHouse != null
                ? Application.Products.DTOs.WareHouseDTO.Init(
                    x.WareHouse.Id ?? "",
                    x.WareHouse.Address ?? "",
                    x.WareHouse.OwnerName ?? "",
                    x.WareHouse.OwnerEmail ?? "",
                    x.WareHouse.OwnerPhone ?? "")
                : null;
                return GetBatchInventoryDTO.Init(
                    x.ProductId,
                    x.StockQuantity,
                    x.CorrelationId ?? "",
                    x.WareHouseId ?? "",
                    warehouseDto
                );
            }).ToList();
        }
        catch (RpcException ex)
        {
            throw new Exception($"gRPC communication failed. Status code: {ex.StatusCode}. Details: {ex.Status.Detail}");
        }
    }

        public async Task CallInventoryToUpdateGrpc(string productId, string wareHouseId, int newStock, Guid updateEventId, CancellationToken cancellationToken = default)
    {
        var path = $"api/inventories/{productId}";
        var payload = new { WareHouseId = wareHouseId, Stock = newStock, UpdateEventId = updateEventId };

        var response = await SendInternalRequestAsync(HttpMethod.Put, path, payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Update failed: {response.StatusCode} - {error}");
        }
    }
}