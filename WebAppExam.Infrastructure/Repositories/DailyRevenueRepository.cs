using System;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;

namespace WebAppExam.Infrastructure.Repositories;

public class DailyRevenueRepository : Repository<DailyRevenue>, IDailyRevenueRepository
{
    public DailyRevenueRepository(AppDbContext context) : base(context)
    {
    }
}
