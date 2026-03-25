namespace WebAppExam.Application.Common.Interfaces;

public interface IInventoryService
{
    Task<bool> CheckStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);

    Task ReserveStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);

    Task ReleaseStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}