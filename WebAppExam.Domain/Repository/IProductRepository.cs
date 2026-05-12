using WebAppExam.Domain;
using WebAppExam.Domain.Common;

namespace WebAppExam.Domain.Repository;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetByNameAsync(string name);
    IQueryable<Product> SearchProductNameQuery(IQueryable<Product> query, string searchTerm);
    IQueryable<Product> GetProductsNotSync();
    IQueryable<Product> GetProductByIdsQuery(List<Ulid> ids);
    IQueryable<Product> GetProductByIdsAndWareHouseIdsQuery(List<Ulid> ids, List<string> wareHouseIds);
}   
