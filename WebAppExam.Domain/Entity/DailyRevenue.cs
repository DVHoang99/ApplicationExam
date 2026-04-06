using System;

namespace WebAppExam.Domain.Entity;

public class DailyRevenue : EntityBase
{
    public string Date { get; private set; }
    public int TotalOrders { get; private set; }
    public int TotalRevenue { get; private set; }

    private DailyRevenue() { }

    public DailyRevenue(DateTime date, int totalOrders, int totalRevenue)
    {
        Date = date.Date.ToString("yyyy-MM-dd");
        TotalOrders = totalOrders;
        TotalRevenue = totalRevenue;
        UpdatedAt = DateTime.UtcNow;
    }
    public void AddDailyRevenue(int totalOrders, int totalRevenue)
    {
        TotalOrders += totalOrders;
        TotalRevenue += totalRevenue;
        UpdatedAt = DateTime.UtcNow;
    }
}
