using Microsoft.EntityFrameworkCore.Storage;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Repositories;
using KafkaFlow.Producers;
using WebAppExam.Application.Logger.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebAppExam.Application.Services;

namespace WebAppExam.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private IProductRepository? _products;
    private IOrderRepository? _orders;
    private IInventoryRepository? _inventory;
    private ICustomerRepository? _customers;
    private readonly IProducerAccessor _producerAccessor;
    private readonly ICurrentUserService _currentUserService;

    public UnitOfWork(AppDbContext context, IProducerAccessor producerAccessor, ICurrentUserService currentUserService)
    {
        _context = context;
        _producerAccessor = producerAccessor;
        _currentUserService = currentUserService;
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
            await SaveChangesAsync();
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

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = GetAuditEntries();
        var result = await _context.SaveChangesAsync(cancellationToken);
        await PublishAuditLogsAsync(auditEntries);

        return result;
    }

    private async Task PublishAuditLogsAsync(List<AuditLogMessageDTO> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0) return;

        var producer = _producerAccessor.GetProducer("system-logs-producer");

        foreach (var log in auditEntries)
        {
            await producer.ProduceAsync(
                "system-logs-topic",
                Guid.NewGuid().ToString(),
                log
            );
        }
    }

    private List<AuditLogMessageDTO> GetAuditEntries()
    {
        _context.ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditLogMessageDTO>();

        // Filter entities that are Added, Modified, or Deleted
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var auditMessage = new AuditLogMessageDTO
            {
                EntityName = entry.Metadata.Name.Split('.').Last(),
                Timestamp = DateTime.UtcNow,
                ChangedBy = _currentUserService.UserId,
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary) continue;

                string propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                {
                    auditMessage.PrimaryKey = property.CurrentValue?.ToString() ?? string.Empty;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditMessage.Action = "Create";
                        newValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditMessage.Action = "Delete";
                        oldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        // Only track columns that actually changed
                        if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                        {
                            auditMessage.Action = "Update";
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            if (entry.State == EntityState.Modified && oldValues.Count == 0)
            {
                continue;
            }

            auditMessage.OldValues = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null;
            auditMessage.NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null;

            auditEntries.Add(auditMessage);
        }

        return auditEntries;
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