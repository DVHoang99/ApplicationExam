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

    public async Task<List<Product>> GetByNameAsync(string name)
    {
        return await ToListAsync(
            Query()
            .AsNoTracking()
            .Where(x => x.Name.Contains(name))
        );
    }

    public async Task<List<Product>> SearchAsync(string keyword)
    {
        return await ToListAsync(
            Query()
            .AsNoTracking()
            .Where(x => x.Name.Contains(keyword))
        );
    }

    public async Task<bool> ExistsAsync(Ulid id, CancellationToken cancellationToken = default)
    {
        return await Query().AnyAsync(x => x.Id == id, cancellationToken);
    }

    public IQueryable<Product> GetProductByIdsQuery(List<Ulid> ids)
    {
        return Query()
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.DeletedAt == null);
    }

    public IQueryable<Product> GetProductByIdsAndWareHouseIdsQuery(List<Ulid> ids, List<string> wareHouseIds)
    {
        return Query()
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id) &&
            x.DeletedAt == null &&
            wareHouseIds.Contains(x.WareHouseId));
    }

    public IQueryable<Product> SearchProductNameQuery(IQueryable<Product> query, string searchTerm)
    {
        return query.Where(x => EF.Functions.ILike(
            AppDbContext.FUnaccent(x.Name),
            AppDbContext.FUnaccent($"%{searchTerm}%")
        ));
    }

    public IQueryable<Product> GetProductsNotSync()
    {
        return Query()
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.CorrelationId) &&
            x.ProductStatus == Domain.Enum.ProductStatus.Pending &&
            x.DeletedAt == null &&
            x.CreatedAt >= DateTime.UtcNow.Date && x.CreatedAt < DateTime.UtcNow.AddMinutes(-5));
    }
}