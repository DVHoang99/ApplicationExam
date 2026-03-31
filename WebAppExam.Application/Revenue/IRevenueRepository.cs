using System;

namespace WebAppExam.Application.Revenue;

public interface IRevenueRepository
{
    Task UpsertMonthlyRevenueAsync(DateTime occurredOn, decimal amount, int counter, CancellationToken cancellationToken = default);
}
