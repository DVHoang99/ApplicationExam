using WebAppExam.Domain.Common;

namespace WebAppExam.Domain.Repository;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetByNameAsync(string name);
    Task<Dictionary<Ulid, Product>> GetProductByIdsAsync(List<Ulid> ids, CancellationToken cancellationToken = default);
    IQueryable<Product> Include(IQueryable<Product> query);
    IQueryable<Product> SearchProductNameQuery(IQueryable<Product> query, string searchTerm);
    Task<List<Product>> ToListAsync(IQueryable<Product> query, CancellationToken cancellationToken = default);
}
