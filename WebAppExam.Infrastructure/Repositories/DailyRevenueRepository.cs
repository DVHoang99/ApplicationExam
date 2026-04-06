using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;

namespace WebAppExam.Infrastructure.Repositories;

public class DailyRevenueRepository : Repository<DailyRevenue>, IDailyRevenueRepository
{
    public DailyRevenueRepository(AppDbContext context) : base(context)
    {


    }
    public async Task<DailyRevenue?> GetByKeyAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Date == id, cancellationToken);
    }
}
