using WebAppExam.Domain.Common;

namespace WebAppExam.Domain.Repository;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetByNameAsync(string name);
}