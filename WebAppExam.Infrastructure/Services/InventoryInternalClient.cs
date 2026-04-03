using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Services;

namespace WebAppExam.Infrastructure.Services;

public class InventoryInternalClient : IInventoryService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public InventoryInternalClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5134/");
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public async Task<InventoryDTO?> CreateInventoryAsync(string wareHouseId, string productId, int stock, string correlationId, CancellationToken cancellationToken = default)
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
            return node?["data"].Deserialize<InventoryDTO>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
}