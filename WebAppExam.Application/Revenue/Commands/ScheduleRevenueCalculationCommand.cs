using MediatR;
using WebAppExam.Application.Services;
using WebAppExam.Application.Revenue;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Revenue.Commands;

/// <summary>
/// Command to schedule revenue calculation jobs
/// </summary>
public class ScheduleRevenueCalculationCommand : ICommand<Unit>
{
}

/// <summary>
/// Handler for scheduling revenue calculation jobs with Hangfire
/// </summary>
public class ScheduleRevenueCalculationCommandHandler : IRequestHandler<ScheduleRevenueCalculationCommand, Unit>
{
    private readonly IHangfireJobService _hangfireJobService;
    private readonly IRevenueCalculationService _revenueCalculationService;

    public ScheduleRevenueCalculationCommandHandler(
        IHangfireJobService hangfireJobService,
        IRevenueCalculationService revenueCalculationService)
    {
        _hangfireJobService = hangfireJobService;
        _revenueCalculationService = revenueCalculationService;
    }

    /// <summary>
    /// Schedule recurring revenue calculation jobs
    /// </summary>
    public async Task<Unit> Handle(ScheduleRevenueCalculationCommand request, CancellationToken cancellationToken)
    {
        // Schedule daily revenue summary calculation at 1:00 AM UTC
        _hangfireJobService.AddOrUpdateRecurring(
            "daily-revenue-summary",
            () => _revenueCalculationService.GetTodayRevenueSummaryAsync(cancellationToken),
            "0 1 * * *" // 1:00 AM every day
        );

        // Schedule monthly revenue summary calculation on the 1st of each month at 2:00 AM UTC
        _hangfireJobService.AddOrUpdateRecurring(
            "monthly-revenue-summary",
            () => _revenueCalculationService.GetCurrentMonthRevenueSummaryAsync(cancellationToken),
            "0 2 1 * *" // 2:00 AM on the 1st of every month
        );

        // Schedule daily revenue calculation at 3:00 AM UTC (for all paid orders)
        _hangfireJobService.AddOrUpdateRecurring(
            "daily-revenue-calculation",
            () => _revenueCalculationService.CalculateDailyRevenueAsync(DateTime.UtcNow, cancellationToken),
            "0 3 * * *" // 3:00 AM every day
        );

        return Unit.Value;
    }
}
