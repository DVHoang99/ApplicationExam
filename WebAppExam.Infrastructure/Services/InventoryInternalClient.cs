using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Hangfire;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Services;

namespace WebAppExam.Infrastructure.Services;

public class InventoryInternalClient : IInventoryService
{
    private readonly HttpClient _httpClient;

    public InventoryInternalClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5134/");
    }
    public async Task<InventoryDTO?> CreateInventoryAsync(string wareHouseId, string productId, int stock, string correlationId, CancellationToken cancellationToken = default)
    {
        var path = $"api/inventories";

        var payload = new
        {
            WareHouseId = wareHouseId,
            ProductId = productId,
            StockQuantity = stock,
            CorrelationId = correlationId
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(path, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                var node = JsonNode.Parse(jsonString);
                var dataNode = node?["data"];

                if (dataNode == null) return null;

                return dataNode.Deserialize<InventoryDTO>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Error call Service B: HTTP {(int)response.StatusCode} - {response.ReasonPhrase}. Chi tiết: {errorContent}");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Can't connect to Service B: {ex.Message}");
        }
    }

    public async Task<List<GetBatchInventoryDTO>> GetInventoryDTOsAsync(List<string> correlationIds, CancellationToken cancellationToken = default)
    {
        var path = "api/inventories/batch";

        var payload = new
        {
            CorrelationIds = correlationIds
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(path, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                var node = JsonNode.Parse(jsonString);
                var dataNode = node?["data"];

                if (dataNode == null) return new List<GetBatchInventoryDTO>();

                return dataNode.Deserialize<List<GetBatchInventoryDTO>>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<GetBatchInventoryDTO>();
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Error call Service B: HTTP {(int)response.StatusCode} - {response.ReasonPhrase}. Chi tiết: {errorContent}");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Can't connect to Service B: {ex.Message}");
        }
    }

    public async Task CallInventoryToUpdate(string productId, string wareHouseId, int newStock, Guid updateEventId, CancellationToken cancellationToken = default)
    {
        var path = $"api/inventories/{productId}";
        var payload = new
        {
            WareHouseId = wareHouseId,
            Stock = newStock,
            UpdateEventId = updateEventId
        };

        try
        {
            var response = await _httpClient.PutAsJsonAsync(path, payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Update not success. Status: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Can't connect to Service B: {ex.Message}");
        }
    }

    public async Task CallInventoryToDelete(string productId, string wareHouseId, CancellationToken cancellationToken = default)
    {
        var path = $"api/inventories/{productId}?wareHouseId={wareHouseId}";

        try
        {
            var response = await _httpClient.DeleteAsync(path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Delete failed. Status: {(int)response.StatusCode}. Detail: {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Can't connect to Service B: {ex.Message}");
        }
    }
}
