using KafkaFlow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Infrastructure.Persistence.AppicationDbContext;

namespace WebAppExam.Application.Revenue;

public class RevenueUpdateHandler : IMessageHandler<OrderCreatedIntegrationEvent>
{
    private readonly IServiceProvider _serviceProvider;

    public RevenueUpdateHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(IMessageContext context, OrderCreatedIntegrationEvent message)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var monthYear = message.OccurredOn.ToString("yyyy-MM");

        var sql = @"
            INSERT INTO ""MonthlyRevenues"" (""MonthYear"", ""TotalOrders"", ""TotalRevenue"")
            VALUES (@monthYear, 1, @amount)
            ON CONFLICT (""MonthYear"") DO UPDATE SET 
                ""TotalOrders"" = ""MonthlyRevenues"".""TotalOrders"" + 1,
                ""TotalRevenue"" = ""MonthlyRevenues"".""TotalRevenue"" + @amount;";

        await dbContext.Database.ExecuteSqlRawAsync(sql,
            new NpgsqlParameter("@monthYear", monthYear),
            new NpgsqlParameter("@amount", message.Amount));

        Console.WriteLine($"[Kafka] Đã xử lý doanh thu cho đơn hàng: {message.OrderId}");
    }
}
