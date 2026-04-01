using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;

namespace WebAppExam.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {

    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public void Delete(Product product)
    {
        _context.Products.Remove(product);
    }

    public new async Task<Product?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            //.Include(p => p.Inventories)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
    }

    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products.ToListAsync(cancellationToken);
    }

    public async Task<List<Product>> GetByNameAsync(string name)
    {
        return await _context.Products
            .Where(x => x.Name.Contains(name))
            .ToListAsync();
    }

    public async Task<List<Product>> SearchAsync(string keyword)
    {
        return await _context.Products
            .Where(x => x.Name.Contains(keyword))
            .ToListAsync();
    }
    public async Task<bool> ExistsAsync(Ulid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Dictionary<Ulid, Product>> GetProductByIdsAsync(List<Ulid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Products
        //.Include(p => p.Inventories)
            .Where(x => ids.Contains(x.Id) &&
            x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x, cancellationToken);
    }

    public IQueryable<Product> SearchProductNameQuery(IQueryable<Product> query, string searchTerm)
    {
        return query.Where(x => EF.Functions.ILike(
            AppDbContext.FUnaccent(x.Name),
            AppDbContext.FUnaccent($"%{searchTerm}%")
        ));
    }

    public Task<List<Product>> ToListAsync(IQueryable<Product> query, CancellationToken cancellationToken = default)
    {
        return query.ToListAsync(cancellationToken);
    }
    public async Task<List<Product>> GetProductsNotSync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
        .Where(x => !string.IsNullOrWhiteSpace(x.CorrelationId) &&
        x.ProductStatus == Domain.Enum.ProductStatus.Pending &&
        x.DeletedAt == null &&
        x.CreatedAt >= DateTime.UtcNow.Date && x.CreatedAt < DateTime.UtcNow.AddMinutes(-5)).ToListAsync(cancellationToken);
    }
}