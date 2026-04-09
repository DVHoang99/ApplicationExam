using System;

namespace WebAppExam.Application.Services;

/// <summary>
/// Interface for revenue calculation services
/// </summary>
public interface IRevenueCalculationService
{
    /// <summary>
    /// Calculate daily revenue for a specific date
    /// </summary>
    Task<decimal> CalculateDailyRevenueAsync(DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate monthly revenue for a specific month
    /// </summary>
    Task<decimal> CalculateMonthlyRevenueAsync(int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate yearly revenue for a specific year
    /// </summary>
    Task<decimal> CalculateYearlyRevenueAsync(int year, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate total revenue for a date range
    /// </summary>
    Task<decimal> CalculateRevenueRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get revenue summary for the current day
    /// </summary>
    Task<RevenueSummaryDto> GetTodayRevenueSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get revenue summary for the current month
    /// </summary>
    Task<RevenueSummaryDto> GetCurrentMonthRevenueSummaryAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Revenue summary data transfer object
/// </summary>
public class RevenueSummaryDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}
