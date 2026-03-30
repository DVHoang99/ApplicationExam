using System;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebAppExam.Application.Revenue;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;

namespace WebAppExam.Infrastructure.Repositories;

public class RevenueRepository : IRevenueRepository
{
    private readonly AppDbContext _dbContext;

    public RevenueRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpsertMonthlyRevenueAsync(DateTime occurredOn, decimal amount, CancellationToken cancellationToken = default)
    {
        var monthYear = occurredOn.ToString("yyyy-MM");

        var sql = @"
            INSERT INTO ""monthly_revenues"" (""MonthYear"", ""TotalOrders"", ""TotalRevenue"")
            VALUES (@monthYear, 1, @amount)
            ON CONFLICT (""MonthYear"") DO UPDATE SET 
                ""TotalOrders"" = ""monthly_revenues"".""TotalOrders"" + 1,
                ""TotalRevenue"" = ""monthly_revenues"".""TotalRevenue"" + @amount;";

        await _dbContext.Database.ExecuteSqlRawAsync(sql,
            new NpgsqlParameter("@monthYear", monthYear),
            new NpgsqlParameter("@amount", amount));
    }
}
