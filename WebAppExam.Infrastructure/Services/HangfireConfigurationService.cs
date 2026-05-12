using Hangfire;
using Microsoft.Extensions.Logging;
using WebAppExam.Application.Services;
using WebAppExam.Application.OutboxMessages;

namespace WebAppExam.Infrastructure.Services;

/// <summary>
/// Hangfire configuration and job initialization service
/// </summary>
public class HangfireConfigurationService : IHangfireConfigurationService
{
    private readonly IHangfireJobService _hangfireJobService;
    private readonly IRevenueCalculationService _revenueCalculationService;
    private readonly IOutboxService _outboxService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<HangfireConfigurationService> _logger;
    private const string DashboardUrl = "/hangfire";

    public HangfireConfigurationService(
        IHangfireJobService hangfireJobService,
        IRevenueCalculationService revenueCalculationService,
        IOutboxService outboxService,
        IBackgroundJobClient backgroundJobClient,
        ILogger<HangfireConfigurationService> logger)
    {
        _hangfireJobService = hangfireJobService;
        _revenueCalculationService = revenueCalculationService;
        _outboxService = outboxService;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    /// <summary>
    /// Initialize and schedule all recurring background jobs
    /// </summary>
    public async Task InitializeRecurringJobsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initializing Hangfire recurring jobs...");

            // Daily revenue calculation at 1:00 AM UTC
            _hangfireJobService.AddOrUpdateRecurring(
                "daily-revenue-calculation",
                () => _revenueCalculationService.CalculateDailyRevenueAsync(DateTime.UtcNow, cancellationToken),
                "0 1 * * *"  // 1:00 AM every day
            );
            _logger.LogInformation("Scheduled recurring job: daily-revenue-calculation at 1:00 AM UTC");

            // Monthly revenue summary on the 1st at 2:00 AM UTC
            _hangfireJobService.AddOrUpdateRecurring(
                "monthly-revenue-summary",
                () => _revenueCalculationService.GetCurrentMonthRevenueSummaryAsync(cancellationToken),
                "0 2 1 * *"  // 2:00 AM on the 1st of every month
            );
            _logger.LogInformation("Scheduled recurring job: monthly-revenue-summary on 1st at 2:00 AM UTC");

            // Weekly revenue report on Sunday at 3:00 AM UTC
            _hangfireJobService.AddOrUpdateRecurring(
                "weekly-revenue-summary",
                () => _revenueCalculationService.GetTodayRevenueSummaryAsync(cancellationToken),
                "0 3 * * 0"  // 3:00 AM on Sunday
            );
            _logger.LogInformation("Scheduled recurring job: weekly-revenue-summary on Sunday at 3:00 AM UTC");

            // Process failed/pending outbox messages every minute (Safety net)
            _hangfireJobService.AddOrUpdateRecurring(
                "process-outbox-messages",
                () => _outboxService.ProcessPendingMessagesAsync(cancellationToken),
                "* * * * *" // Every minute
            );
            _logger.LogInformation("Scheduled recurring job: process-outbox-messages every minute");

            _logger.LogInformation("Hangfire recurring jobs initialized successfully");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Hangfire recurring jobs");
            throw;
        }
    }

    /// <summary>
    /// Get the Hangfire dashboard URL
    /// </summary>
    public string GetDashboardUrl()
    {
        return DashboardUrl;
    }

    /// <summary>
    /// Check if Hangfire server is running
    /// </summary>
    public bool IsHangfireServerRunning()
    {
        try
        {
            // Try to enqueue a test job to check if server is running
            var jobId = _backgroundJobClient.Enqueue(() => Task.CompletedTask);
            if (!string.IsNullOrEmpty(jobId))
            {
                // Delete the test job
                BackgroundJob.Delete(jobId);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Hangfire server health check failed");
        }

        return false;
    }
}
