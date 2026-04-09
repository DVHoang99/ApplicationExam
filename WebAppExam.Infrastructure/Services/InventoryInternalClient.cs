using System;
using FluentResults;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public InventoryInternalClient(
        IHttpClientService httpClientService,
        InventoryGrpc.InventoryGrpcClient inventoryGrpcClient,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _httpClientService = httpClientService;
        _httpClientService.SetBaseAddress(configuration.GetSection("InternalService")["InventoryService"] ?? "http://localhost:5134/");
        _inventoryGrpcClient = inventoryGrpcClient;
        _httpContextAccessor = httpContextAccessor;
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

            // Extract JWT token from incoming request and forward to gRPC call
            var authorizationHeader = _httpContextAccessor?.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                headers.Add("Authorization", authorizationHeader);
            }

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

    public async Task<Result<List<GetBatchInventoryDTO>>> GetInventoryDTOsByCorrelationIdsGrpcAsync(List<string> correlationIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetByCorrelationIdsRequest();

            if (correlationIds != null && correlationIds.Any())
            {
                request.CorrelationIds.AddRange(correlationIds);
            }

            var headers = new Metadata();

            // Extract JWT token from incoming request and forward to gRPC call
            var authorizationHeader = _httpContextAccessor?.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                headers.Add("Authorization", authorizationHeader);
            }

            var response = await _inventoryGrpcClient.GetByCorrelationIdsAsync(request, headers: headers, cancellationToken: cancellationToken);

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

    private GetBatchInventoryDTO MapProtoInventoryToDTO(Protos.InventoryDTO x)
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
    }

    public async Task<Result<GetBatchInventoryDTO>> GetInventoryGrpcAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetInventoryRequest { Id = id };

            var headers = new Metadata();

            // Extract JWT token from incoming request and forward to gRPC call
            var authorizationHeader = _httpContextAccessor?.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                headers.Add("Authorization", authorizationHeader);
            }

            var response = await _inventoryGrpcClient.GetInventoryAsync(request, headers: headers, cancellationToken: cancellationToken);

            if (response?.Inventory == null)
                return Result.Fail(ExternalServiceError.InventoryServiceError(404, "Inventory not found"));

            var result = MapProtoInventoryToDTO(response.Inventory);
            return Result.Ok(result);
        }
        catch (RpcException ex)
        {
            var statusCode = (int)ex.StatusCode;
            return Result.Fail(ExternalServiceError.InventoryServiceError(statusCode, ex.Status.Detail));
        }
    }

    public async Task<Result<GetBatchInventoryDTO>> CreateInventoryGrpcAsync(string productId, string wareHouseId, int stock, string correlationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var protoInventory = new Protos.InventoryDTO
            {
                ProductId = productId,
                StockQuantity = stock,
                CorrelationId = correlationId ?? "",
                WareHouseId = wareHouseId ?? ""
            };

            var request = new CreateInventoryRequest { Inventory = protoInventory };

            var headers = new Metadata();

            // Extract JWT token from incoming request and forward to gRPC call
            var authorizationHeader = _httpContextAccessor?.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                headers.Add("Authorization", authorizationHeader);
            }

            var response = await _inventoryGrpcClient.CreateInventoryAsync(request, headers: headers, cancellationToken: cancellationToken);

            if (response?.Inventory == null)
                return Result.Fail(ExternalServiceError.InventoryServiceError(500, "Failed to create inventory"));

            var result = MapProtoInventoryToDTO(response.Inventory);
            return Result.Ok(result);
        }
        catch (RpcException ex)
        {
            var statusCode = (int)ex.StatusCode;
            return Result.Fail(ExternalServiceError.InventoryServiceError(statusCode, ex.Status.Detail));
        }
    }

    public async Task<Result<GetBatchInventoryDTO>> UpdateInventoryGrpcAsync(string id, string wareHouseId, int stock, Guid updateEventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new UpdateInventoryRequest
            {
                Id = id,
                WareHouseId = wareHouseId,
                Stock = stock,
                UpdateEventId = updateEventId.ToString()
            };

            var headers = new Metadata();

            // Extract JWT token from incoming request and forward to gRPC call
            var authorizationHeader = _httpContextAccessor?.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                headers.Add("Authorization", authorizationHeader);
            }

            var response = await _inventoryGrpcClient.UpdateInventoryAsync(request, headers: headers, cancellationToken: cancellationToken);

            if (response?.Inventory == null)
                return Result.Fail(ExternalServiceError.InventoryServiceError(500, "Failed to update inventory"));

            var result = MapProtoInventoryToDTO(response.Inventory);
            return Result.Ok(result);
        }
        catch (RpcException ex)
        {
            var statusCode = (int)ex.StatusCode;
            return Result.Fail(ExternalServiceError.InventoryServiceError(statusCode, ex.Status.Detail));
        }
    }

    public async Task<Result> DeleteInventoryGrpcAsync(string id, string wareHouseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteInventoryRequest { Id = id, WareHouseId = wareHouseId };

            var headers = new Metadata();

            // Extract JWT token from incoming request and forward to gRPC call
            var authorizationHeader = _httpContextAccessor?.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                headers.Add("Authorization", authorizationHeader);
            }

            var response = await _inventoryGrpcClient.DeleteInventoryAsync(request, headers: headers, cancellationToken: cancellationToken);

            if (!response.Success)
                return Result.Fail(ExternalServiceError.InventoryServiceError(500, response.Message ?? "Failed to delete inventory"));

            return Result.Ok();
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