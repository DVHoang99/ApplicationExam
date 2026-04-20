using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain;
using WebAppExam.Domain.Common;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;

namespace WebAppExam.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (typeof(EntityBase).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(x => EF.Property<DateTime?>(x, "DeletedAt") == null);
        }

        return await query.FirstOrDefaultAsync(x =>
            EF.Property<Ulid>(x, "Id") == id, cancellationToken);
    }

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (typeof(EntityBase).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(x => EF.Property<DateTime?>(x, "DeletedAt") == null);
        }

        return await query
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
    public async Task AddRangeAsync(List<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
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

    public async Task<List<T>> ToListAsync(IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        return await query.ToListAsync(cancellationToken);
    }

    public IQueryable<T> FromSqlInterpolated(FormattableString sql)
    {
        return _dbSet.FromSqlInterpolated(sql);
    }
}