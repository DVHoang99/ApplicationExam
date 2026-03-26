using Microsoft.EntityFrameworkCore.Storage;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Repositories;

namespace WebAppExam.Infrastructure.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IInventoryRepository Inventory { get; }
    ICustomerRepository Customers { get; }


    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private IProductRepository? _products;
    private IOrderRepository? _orders;
    private IInventoryRepository? _inventory;
    private ICustomerRepository? _customers;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public IInventoryRepository Inventory => _inventory ??= new InventoryRepository(_context);
    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);


    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync()
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }


    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();
        await _context.DisposeAsync();
    }
}