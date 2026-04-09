using WebAppExam.Infrastructure.Extensions;

// Add to your Program.cs file

// ============================================
// SECTION 1: Service Registration (in builder setup)
// ============================================
// This is already done in DependencyInjection.cs:
// services.AddInfrastructure(configuration);
// which includes:
// - services.AddScoped<IHangfireConfigurationService, HangfireConfigurationService>();
// - services.AddHangfireServer();


// ============================================
// Build the application
// ============================================
var app = builder.Build();


// ============================================
// SECTION 2: Configure Middleware (Order matters!)
// ============================================

// First, configure general middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// === HANGFIRE CONFIGURATION (Add these lines) ===

// Add Hangfire Dashboard with authorization
app.UseHangfireConfigured("/hangfire");  // or use default: app.UseHangfireConfigured()

// Initialize Hangfire recurring jobs
await app.InitializeHangfireJobsAsync();

// Optional: Check Hangfire server health
if (app.IsHangfireHealthy())
{
    app.Logger.LogInformation("✓ Hangfire server is running and healthy");
}
else
{
    app.Logger.LogWarning("⚠ Hangfire server may not be running");
}

// === END HANGFIRE CONFIGURATION ===

// Continue with other middleware
app.UseAuthorization();
app.MapControllers();

app.Run();


// ============================================
// SECTION 3: How to Use
// ============================================

/*
 * ACCESS HANGFIRE DASHBOARD:
 * - URL: https://localhost:5001/hangfire (or http://localhost:5000/hangfire in development)
 * - Authorization: Allowed in Development, requires Admin role in Production
 * 
 * WHAT YOU'LL SEE:
 * - Recurring Jobs: daily-revenue-calculation, monthly-revenue-summary, weekly-revenue-summary
 * - Job History: Past job executions with success/failure status
 * - Servers: Active Hangfire servers
 * - Job Queues: Current job queue status
 * - Failed Jobs: Any failed jobs for debugging
 * 
 * FEATURES:
 * - Real-time job monitoring
 * - View job details and logs
 * - Manually trigger jobs
 * - Retry failed jobs
 * - Set up job alerts
 */


// ============================================
// SECTION 4: Hangfire Configuration Details
// ============================================

/*
 * DATABASE:
 * - PostgreSQL with Hangfire.PostgreSql
 * - Connection string from appsettings.json: "DefaultConnection"
 * - Automatic job persistence
 * 
 * RECURRING JOBS (Scheduled):
 * 1. daily-revenue-calculation
 *    - Time: 1:00 AM UTC
 *    - Frequency: Every day
 *    - Action: Calculate daily revenue for all paid orders
 * 
 * 2. monthly-revenue-summary
 *    - Time: 2:00 AM UTC on the 1st
 *    - Frequency: Every month
 *    - Action: Generate revenue summary for current month
 * 
 * 3. weekly-revenue-summary
 *    - Time: 3:00 AM UTC on Sundays
 *    - Frequency: Every week
 *    - Action: Get weekly revenue summary
 * 
 * RETRY POLICY:
 * - Automatic retries: Up to 5 attempts
 * - Exponential backoff between retries
 */


// ============================================
// SECTION 5: Alternative Configuration (Manual)
// ============================================

/*
 * If you prefer manual job scheduling instead of automatic initialization:
 * 
 * using (var scope = app.Services.CreateScope())
 * {
 *     var hangfireJobService = scope.ServiceProvider
 *         .GetRequiredService<IHangfireJobService>();
 *     var revenueService = scope.ServiceProvider
 *         .GetRequiredService<IRevenueCalculationService>();
 * 
 *     // Daily calculation
 *     hangfireJobService.AddOrUpdateRecurring(
 *         "daily-revenue",
 *         () => revenueService.CalculateDailyRevenueAsync(DateTime.UtcNow, CancellationToken.None),
 *         Cron.Daily(1)
 *     );
 * 
 *     // One-time job (runs immediately)
 *     hangfireJobService.Enqueue(
 *         () => revenueService.CalculateDailyRevenueAsync(DateTime.UtcNow, CancellationToken.None)
 *     );
 * 
 *     // Delayed job (runs in 5 minutes)
 *     hangfireJobService.Schedule(
 *         () => revenueService.CalculateDailyRevenueAsync(DateTime.UtcNow, CancellationToken.None),
 *         TimeSpan.FromMinutes(5)
 *     );
 * }
 */


// ============================================
// SECTION 6: Troubleshooting
// ============================================

/*
 * ISSUE: Dashboard shows no jobs
 * SOLUTION: Make sure InitializeHangfireJobsAsync() is called before app.Run()
 * 
 * ISSUE: Jobs not executing
 * SOLUTION: 
 * - Verify Hangfire server is running (check /hangfire/servers)
 * - Check database connection in appsettings.json
 * - Check job logs in /hangfire dashboard
 * - Ensure time zones are correct (using UTC)
 * 
 * ISSUE: "Access Denied" on dashboard
 * SOLUTION:
 * - In Development: Already allowed
 * - In Production: Configure authorization filter in HangfireMiddlewareExtensions
 * - Check HangfireAuthorizationFilter implementation
 * 
 * ISSUE: Database connection errors
 * SOLUTION:
 * - Verify PostgreSQL is running
 * - Check connection string in appsettings.json
 * - Ensure Hangfire schema is created:
 *   - Hangfire creates tables automatically on first run
 *   - Tables: HangfireCounter, HangfireHash, HangfireJob, etc.
 */


// ============================================
// SECTION 7: Monitoring & Health Checks
// ============================================

/*
 * Built-in Health Check:
 * bool isHealthy = app.IsHangfireHealthy();
 * 
 * Manual Monitoring:
 * - Access /hangfire/servers to see active servers
 * - Check job processing count
 * - Monitor failed jobs queue
 * - Set up external monitoring (e.g., Application Insights)
 * 
 * Logging:
 * - All job execution logged to ILogger<HangfireConfigurationService>
 * - View logs: var logger = serviceProvider.GetRequiredService<ILogger<...>>()
 * - Search for "Hangfire" in application logs
 */
