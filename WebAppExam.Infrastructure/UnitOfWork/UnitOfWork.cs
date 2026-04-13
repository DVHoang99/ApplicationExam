using Microsoft.EntityFrameworkCore.Storage;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Repositories;
using KafkaFlow.Producers;
using WebAppExam.Application.Logger.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebAppExam.Application.Services;
using MediatR;
using WebAppExam.Domain;
using WebAppExam.Infrastructure.Exceptions;

namespace WebAppExam.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private IProductRepository? _products;
    private IOrderRepository? _orders;
    //private IInventoryRepository? _inventory;
    private ICustomerRepository? _customers;
    private readonly IProducerAccessor _producerAccessor;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;


    public UnitOfWork(AppDbContext context, IProducerAccessor producerAccessor, ICurrentUserService currentUserService, IMediator mediator)
    {
        _context = context;
        _producerAccessor = producerAccessor;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    //public IInventoryRepository Inventory => _inventory ??= new InventoryRepository(_context);
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
                var auditEntries = GetAuditEntries();
                await _transaction.CommitAsync();
                await PublishEvents();
                await PublishAuditLogsAsync(auditEntries);
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
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new DatabaseOperationException("Failed to save changes to the database.", ex);
        }
    }
    private async Task PublishEvents(CancellationToken ct = default)
    {
        var entitiesWithEvents = _context.ChangeTracker.Entries<EntityBase>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity).ToList();

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var @event in entity.DomainEvents)
            {
                await _mediator.Publish(@event, ct);
            }
            entity.ClearDomainEvents();
        }
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
            var entityName = entry.Metadata.Name.Split('.').Last();
            var timestamp = DateTime.UtcNow;
            var changedBy = _currentUserService.UserId;
            var primaryKey = string.Empty;
            var action = string.Empty;

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary) continue;

                string propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                {
                    primaryKey = property.CurrentValue?.ToString();
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        action = "Create";
                        newValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        action = "Delete";
                        oldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        // Only track columns that actually changed
                        if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                        {
                            action = "Update";
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

            var oldValuesString = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null;
            var newValuesString = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null;

            var auditMessage = AuditLogMessageDTO.FromResult(
                entityName,
                action,
                primaryKey,
                oldValuesString,
                newValuesString
            );

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