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

    public async Task<Product?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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

    public async Task<List<Product>> GetAvailableProductsAsync()
    {
        return await _context.Products
            .Where(x => x.Stock > 0)
            .ToListAsync();
    }

    public async Task<bool> IsInStockAsync(Ulid productId, int quantity)
    {
        return await _context.Products
            .Where(x => x.Id == productId)
            .AnyAsync(x => x.Stock >= quantity);
    }

    public async Task<bool> ExistsAsync(Ulid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.AnyAsync(x => x.Id == id, cancellationToken);
    }
}