using WebAppExam.Domain.Repository;

public interface IUnitOfWork : IAsyncDisposable
{
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}