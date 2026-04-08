using System;
using WebAppExam.Application.Products.DTOs;

namespace WebAppExam.Application.Products.Services;

public interface IInventoryService
{
    Task<InventoryDTO?> CreateInventoryAsync(string wareHouseId, string productId, int stock, string correlationId, CancellationToken cancellationToken = default);
    Task<List<GetBatchInventoryDTO>> GetInventoryDTOsAsync(List<string> correlationIds, CancellationToken cancellationToken = default);
    Task CallInventoryToUpdate(string productId, string wareHouseId, int stock, Guid updateEventId, CancellationToken cancellationToken = default);
    Task CallInventoryToDelete(string productId, string wareHouseId, CancellationToken cancellationToken = default);
    Task<List<GetBatchInventoryDTO>?> GetInventoryDTOsByIdsAsync(List<string> ids, CancellationToken cancellationToken = default);
    Task CallInventoryToUpdateGrpc(string productId, string wareHouseId, int newStock, Guid updateEventId, CancellationToken cancellationToken = default)
}
