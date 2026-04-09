using System;
using FluentResults;
using Grpc.Core;
using WebAppExam.Application.Common.Errors;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Services;
using WebAppExam.Application.Services;
using WebAppExam.Infrastructure.Protos;

namespace WebAppExam.Infrastructure.Services;

public class InventoryInternalClient : IInventoryService
{
    private readonly IHttpClientService _httpClientService;
    private readonly InventoryGrpc.InventoryGrpcClient _inventoryGrpcClient;

    public InventoryInternalClient(
        IHttpClientService httpClientService,
        InventoryGrpc.InventoryGrpcClient inventoryGrpcClient)
    {
        _httpClientService = httpClientService;
        _httpClientService.SetBaseAddress("http://localhost:5134/");
        _inventoryGrpcClient = inventoryGrpcClient;
    }

    public async Task<Result<Application.Products.DTOs.InventoryDTO>> CreateInventoryAsync(string wareHouseId, string productId, int stock, string correlationId, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            WareHouseId = wareHouseId,
            ProductId = productId,
            StockQuantity = stock,
            CorrelationId = correlationId
        };

        var dto = await _httpClientService.SendAsync<Application.Products.DTOs.InventoryDTO>(
            HttpMethod.Post, "api/inventories", payload, cancellationToken);

        if (dto != null)
        {
            return Result.Ok(dto);
        }

        return Result.Fail(ExternalServiceError.InventoryServiceError(500, "Failed to create inventory"));
    }

    public async Task<Result<List<GetBatchInventoryDTO>>> GetInventoryDTOsAsync(List<string> correlationIds, CancellationToken cancellationToken = default)
    {
        var payload = new { CorrelationIds = correlationIds };
        var result = await _httpClientService.SendAsync<List<GetBatchInventoryDTO>>(
            HttpMethod.Post, "api/inventories/batch", payload, cancellationToken);

        return Result.Ok(result ?? new List<GetBatchInventoryDTO>());
    }

    public async Task<Result> CallInventoryToUpdate(string productId, string wareHouseId, int newStock, Guid updateEventId, CancellationToken cancellationToken = default)
    {
        var path = $"api/inventories/{productId}";
        var payload = new { WareHouseId = wareHouseId, Stock = newStock, UpdateEventId = updateEventId };

        try
        {
            var response = await _httpClientService.SendAsync(HttpMethod.Put, path, payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result.Fail(ExternalServiceError.InventoryServiceError((int)response.StatusCode, error));
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ExternalServiceError.InventoryServiceError(500, ex.Message));
        }
    }

    public async Task<Result> CallInventoryToDelete(string productId, string wareHouseId, CancellationToken cancellationToken = default)
    {
        var path = $"api/inventories/{productId}?wareHouseId={wareHouseId}";

        try
        {
            var response = await _httpClientService.SendAsync(HttpMethod.Delete, path, null, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result.Fail(ExternalServiceError.InventoryServiceError((int)response.StatusCode, error));
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ExternalServiceError.InventoryServiceError(500, ex.Message));
        }
    }

    public async Task<Result<List<GetBatchInventoryDTO>>> GetInventoryDTOsByIdsAsync(List<string> productIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetBatchInventoriesRequest();

            if (productIds != null && productIds.Any())
            {
                request.Ids.AddRange(productIds);
            }

            var headers = new Metadata();

            var response = await _inventoryGrpcClient.GetBatchInventoriesAsync(request, headers: headers, cancellationToken: cancellationToken);

            if (response?.Inventories == null)
                return Result.Ok(new List<GetBatchInventoryDTO>());

            var result = response.Inventories.Select(x =>
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

            return Result.Ok(result);
        }
        catch (RpcException ex)
        {
            var statusCode = (int)ex.StatusCode;
            return Result.Fail(ExternalServiceError.InventoryServiceError(statusCode, ex.Status.Detail));
        }
    }

    public async Task<Result> CallInventoryToUpdateGrpc(string productId, string wareHouseId, int newStock, Guid updateEventId, CancellationToken cancellationToken = default)
    {
        var path = $"api/inventories/{productId}";
        var payload = new { WareHouseId = wareHouseId, Stock = newStock, UpdateEventId = updateEventId };

        try
        {
            var response = await _httpClientService.SendAsync(HttpMethod.Put, path, payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result.Fail(ExternalServiceError.InventoryServiceError((int)response.StatusCode, error));
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ExternalServiceError.InventoryServiceError(500, ex.Message));
        }
    }
}