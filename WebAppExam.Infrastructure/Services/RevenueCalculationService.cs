using System;
using WebAppExam.Application.Services;
using WebAppExam.Application.Revenue;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Infrastructure.Services;

/// <summary>
/// Service for calculating revenue from orders
/// </summary>
public class RevenueCalculationService : IRevenueCalculationService
{
    private readonly IOrderRepository _orderRepository;

    public RevenueCalculationService(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Calculate daily revenue for a specific date
    /// </summary>
    public async Task<decimal> CalculateDailyRevenueAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1).AddTicks(-1);

        return await CalculateRevenueRangeAsync(startDate, endDate, cancellationToken);
    }

    /// <summary>
    /// Calculate monthly revenue for a specific month
    /// </summary>
    public async Task<decimal> CalculateMonthlyRevenueAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        return await CalculateRevenueRangeAsync(startDate, endDate, cancellationToken);
    }

    /// <summary>
    /// Calculate yearly revenue for a specific year
    /// </summary>
    public async Task<decimal> CalculateYearlyRevenueAsync(int year, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31, 23, 59, 59, 999);

        return await CalculateRevenueRangeAsync(startDate, endDate, cancellationToken);
    }

    /// <summary>
    /// Calculate total revenue for a date range
    /// </summary>
    public async Task<decimal> CalculateRevenueRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate, cancellationToken);

        return orders
            .Where(o => o.Status == Domain.Enum.OrderStatus.Paid)
            .Sum(o => o.TotalAmount);
    }

    /// <summary>
    /// Get revenue summary for today
    /// </summary>
    public async Task<RevenueSummaryDto> GetTodayRevenueSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var startDate = today;
        var endDate = today.AddDays(1).AddTicks(-1);

        var orders = await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate, cancellationToken);
        var completedOrders = orders.Where(o => o.Status == Domain.Enum.OrderStatus.Paid).ToList();

        var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
        var orderCount = completedOrders.Count;
        var averageOrderValue = orderCount > 0 ? totalRevenue / orderCount : 0;

        return new RevenueSummaryDto
        {
            PeriodStart = startDate,
            PeriodEnd = endDate,
            TotalRevenue = totalRevenue,
            OrderCount = orderCount,
            AverageOrderValue = averageOrderValue
        };
    }

    /// <summary>
    /// Get revenue summary for the current month
    /// </summary>
    public async Task<RevenueSummaryDto> GetCurrentMonthRevenueSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var startDate = new DateTime(today.Year, today.Month, 1);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        var orders = await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate, cancellationToken);
        var completedOrders = orders.Where(o => o.Status == Domain.Enum.OrderStatus.Paid).ToList();

        var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
        var orderCount = completedOrders.Count;
        var averageOrderValue = orderCount > 0 ? totalRevenue / orderCount : 0;

        return new RevenueSummaryDto
        {
            PeriodStart = startDate,
            PeriodEnd = endDate,
            TotalRevenue = totalRevenue,
            OrderCount = orderCount,
            AverageOrderValue = averageOrderValue
        };
    }
}
