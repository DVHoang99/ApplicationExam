using System;
using FluentResults;
using WebAppExam.Application.Products.DTOs;

namespace WebAppExam.Application.Products.Services;

public interface IInventoryService
{
    Task<Result<InventoryDTO>> CreateInventoryAsync(string wareHouseId, string productId, int stock, string correlationId, CancellationToken cancellationToken = default);
    Task<Result<List<GetBatchInventoryDTO>>> GetInventoryDTOsAsync(List<string> correlationIds, CancellationToken cancellationToken = default);
    Task<Result> CallInventoryToUpdate(string productId, string wareHouseId, int stock, Guid updateEventId, CancellationToken cancellationToken = default);
    Task<Result> CallInventoryToDelete(string productId, string wareHouseId, CancellationToken cancellationToken = default);
    Task<Result<List<GetBatchInventoryDTO>>> GetInventoryDTOsByIdsAsync(List<string> ids, CancellationToken cancellationToken = default);
    Task<Result<List<GetBatchInventoryDTO>>> GetInventoryDTOsByCorrelationIdsGrpcAsync(List<string> correlationIds, CancellationToken cancellationToken = default);
    Task<Result<GetBatchInventoryDTO>> GetInventoryGrpcAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<GetBatchInventoryDTO>> CreateInventoryGrpcAsync(string productId, string wareHouseId, int stock, string correlationId, CancellationToken cancellationToken = default);
    Task<Result<GetBatchInventoryDTO>> UpdateInventoryGrpcAsync(string id, string wareHouseId, int stock, Guid updateEventId, CancellationToken cancellationToken = default);
    Task<Result> DeleteInventoryGrpcAsync(string id, string wareHouseId, CancellationToken cancellationToken = default);
    Task<Result> CallInventoryToUpdateGrpc(string productId, string wareHouseId, int newStock, Guid updateEventId, CancellationToken cancellationToken = default);
}
