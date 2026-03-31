using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain.Common;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;

namespace WebAppExam.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x =>
            EF.Property<Ulid>(x, "Id") == id &&
            EF.Property<DateTime?>(x, "DeletedAt") == null, cancellationToken);
    }

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(x => EF.Property<DateTime?>(x, "DeletedAt") == null)
        .OrderByDescending(x => EF.Property<Ulid>(x, "Id"))
        .ToListAsync(cancellationToken);
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }
    public void UpdateRange(List<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }


    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }
}