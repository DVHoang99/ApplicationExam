using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebAppExam.Application.Services;

namespace WebAppExam.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring Hangfire middleware and UI
/// </summary>
public static class HangfireMiddlewareExtensions
{
    /// <summary>
    /// Add Hangfire middleware to the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="dashboardPath">Path for Hangfire dashboard (default: /hangfire)</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseHangfireConfigured(
        this IApplicationBuilder app,
        string dashboardPath = "/hangfire")
    {
        // Add Hangfire dashboard middleware with authorization
        app.UseHangfireDashboard(dashboardPath, new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() },
            IgnoreAntiforgeryToken = true,
            DarkModeEnabled = false,
            StatsPollingInterval = 2000  // Poll every 2 seconds for updates
        });

        return app;
    }

    /// <summary>
    /// Initialize Hangfire recurring jobs
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>Task</returns>
    public static async Task InitializeHangfireJobsAsync(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var hangfireConfigService = scope.ServiceProvider
                .GetRequiredService<IHangfireConfigurationService>();

            await hangfireConfigService.InitializeRecurringJobsAsync();
        }
    }

    /// <summary>
    /// Get Hangfire server health status
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>True if Hangfire server is running</returns>
    public static bool IsHangfireHealthy(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var hangfireConfigService = scope.ServiceProvider
                .GetRequiredService<IHangfireConfigurationService>();

            return hangfireConfigService.IsHangfireServerRunning();
        }
    }
}

/// <summary>
/// Authorization filter for Hangfire dashboard access
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Allow access in development environment
        if (context.GetHttpContext().RequestServices
            .GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return true;
        }

        // In production, check for authorization header or custom logic
        var httpContext = context.GetHttpContext();
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();

        // Example: Check for admin role (customize as needed)
        return httpContext.User?.IsInRole("Admin") ?? false;
    }
}
