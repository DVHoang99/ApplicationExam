using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;

namespace WebAppExam.Infrastructure.Repositories;

// public class InventoryRepository : Repository<Inventory>, IInventoryRepository
// {
//     public InventoryRepository(AppDbContext context) : base(context)
//     {
//     }

//     public async Task<Inventory?> GetByProductIdAsync(Ulid productId, CancellationToken cancellationToken = default)
//     {
//         return await _dbSet.FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);
//     }

//     public async Task<List<Inventory>> GetLowStockItemsAsync(int threshold, CancellationToken cancellationToken = default)
//     {
//         return await _dbSet
//             .Where(x => x.Stock <= threshold)
//             .ToListAsync(cancellationToken);
//     }
// }