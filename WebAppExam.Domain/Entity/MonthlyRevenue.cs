using System;

namespace WebAppExam.Domain.Entity;

public class MonthlyRevenue
{
    public string MonthYear { get; private set; }
    public int TotalOrders { get; private set; }
    public int TotalRevenue { get; private set; }

    protected MonthlyRevenue() { }

    public MonthlyRevenue(string monthYear, int initialRevenue)
    {
        MonthYear = monthYear;
        TotalOrders = 1;
        TotalRevenue = initialRevenue;
    }
}
