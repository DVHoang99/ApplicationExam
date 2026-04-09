namespace WebAppExam.Application.Services;

/// <summary>
/// Interface for Hangfire configuration and initialization
/// </summary>
public interface IHangfireConfigurationService
{
    /// <summary>
    /// Initialize and schedule recurring background jobs
    /// </summary>
    Task InitializeRecurringJobsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Hangfire dashboard URL
    /// </summary>
    string GetDashboardUrl();

    /// <summary>
    /// Check if Hangfire server is running
    /// </summary>
    bool IsHangfireServerRunning();
}
