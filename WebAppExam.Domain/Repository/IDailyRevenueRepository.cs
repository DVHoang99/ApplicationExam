using System;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain.Repository;

public interface IDailyRevenueRepository : IRepository<DailyRevenue>
{
    Task<DailyRevenue?> GetByKeyAsync(string id, CancellationToken cancellationToken = default);
}
