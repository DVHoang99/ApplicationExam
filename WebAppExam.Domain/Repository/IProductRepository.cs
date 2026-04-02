using WebAppExam.Domain.Common;

namespace WebAppExam.Domain.Repository;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetByNameAsync(string name);
    Task<Dictionary<Ulid, Product>> GetProductByIdsAsync(List<Ulid> ids, CancellationToken cancellationToken = default);
    Task<Dictionary<Ulid, Product>> GetProductByIdsAndWareHouseIdsAsync(List<Ulid> ids, List<string> wareHouseIds, CancellationToken cancellationToken = default);
    //IQueryable<Product> Include(IQueryable<Product> query);
    IQueryable<Product> SearchProductNameQuery(IQueryable<Product> query, string searchTerm);
    Task<List<Product>> ToListAsync(IQueryable<Product> query, CancellationToken cancellationToken = default);
    Task<List<Product>> GetProductsNotSync(CancellationToken cancellationToken = default);

}
