# Hangfire Configuration Layer & UI Setup

## Overview
Created a complete Hangfire configuration layer with UI dashboard, recurring job scheduling, and health monitoring.

## 📁 Files Created

### 1. **IHangfireConfigurationService.cs**
**Location:** `WebAppExam.Application/Services/IHangfireConfigurationService.cs`

Interface for Hangfire configuration:
```csharp
public interface IHangfireConfigurationService
{
    Task InitializeRecurringJobsAsync(CancellationToken cancellationToken = default);
    string GetDashboardUrl();
    bool IsHangfireServerRunning();
}
```

### 2. **HangfireConfigurationService.cs**
**Location:** `WebAppExam.Infrastructure/Services/HangfireConfigurationService.cs`

Implementation with features:
- Automatic job initialization (3 recurring jobs)
- Hangfire server health checks
- Comprehensive logging
- Dashboard URL management

**Scheduled Jobs:**
1. **daily-revenue-calculation** → 1:00 AM UTC (every day)
2. **monthly-revenue-summary** → 2:00 AM UTC (1st of month)
3. **weekly-revenue-summary** → 3:00 AM UTC (every Sunday)

### 3. **HangfireMiddlewareExtensions.cs**
**Location:** `WebAppExam.Infrastructure/Extensions/HangfireMiddlewareExtensions.cs`

Extension methods for Program.cs:
```csharp
app.UseHangfireConfigured("/hangfire")  // Add dashboard with authorization
await app.InitializeHangfireJobsAsync()  // Initialize all recurring jobs
app.IsHangfireHealthy()                  // Check server health
```

**Authorization Filter:**
- Development: ✓ Allows all access
- Production: ✓ Requires Admin role

### 4. **Updated DependencyInjection.cs**
- Added: `IHangfireConfigurationService` registration

## 🚀 Quick Setup in Program.cs

```csharp
using WebAppExam.Infrastructure.Extensions;

var builder = WebApplicationBuilder.CreateBuilder(args);
builder.Services.AddInfrastructure(configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// === HANGFIRE SETUP ===
app.UseHangfireConfigured("/hangfire");
await app.InitializeHangfireJobsAsync();

if (app.IsHangfireHealthy())
{
    app.Logger.LogInformation("✓ Hangfire is healthy");
}

// Continue with other middleware...
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 📊 Hangfire Dashboard

### Access
- **URL:** `https://localhost:5001/hangfire`
- **Default Path:** `/hangfire`
- **Authorization:** Automatic in Development, Admin role required in Production

### Dashboard Features
- **Recurring Jobs:** View all scheduled jobs (daily, monthly, weekly)
- **Job History:** See execution history with success/failure status
- **Real-time Monitoring:** Auto-refresh every 2 seconds
- **Job Details:** View job parameters, execution time, exceptions
- **Manual Actions:**
  - Trigger job immediately
  - Retry failed jobs
  - Delete/disable jobs
  - View job queue status

## 🔧 Configuration Details

### Database
- **Type:** PostgreSQL with Hangfire.PostgreSql
- **Storage:** Automatic persistence to `Hangfire*` tables
- **Connection:** Uses `DefaultConnection` from appsettings.json

### Retry Policy
- **Attempts:** 5 automatic retries
- **Strategy:** Exponential backoff
- **Failure Handling:** Failed jobs visible in dashboard

### Job Polling
- **Interval:** 2 seconds (StatsPollingInterval)
- **Updates:** Real-time dashboard updates
- **Performance:** Optimized for production use

## 📝 Usage Examples

### Example 1: Access Dashboard
```
1. Start application
2. Open: https://localhost:5001/hangfire
3. View ongoing jobs, history, and servers
4. Manually trigger jobs using UI
```

### Example 2: Inject Configuration Service
```csharp
public class AdminController : ControllerBase
{
    private readonly IHangfireConfigurationService _hangfireConfig;

    [HttpGet("hangfire-status")]
    public IActionResult GetHangfireStatus()
    {
        var isHealthy = _hangfireConfig.IsHangfireServerRunning();
        var dashboardUrl = _hangfireConfig.GetDashboardUrl();
        
        return Ok(new 
        { 
            isHealthy, 
            dashboardUrl,
            jobs = "daily-revenue-calculation, monthly-revenue-summary, weekly-revenue-summary"
        });
    }
}
```

### Example 3: Reinitialize Jobs
```csharp
public class AdminController : ControllerBase
{
    private readonly IHangfireConfigurationService _hangfireConfig;

    [HttpPost("reinitialize-jobs")]
    public async Task<IActionResult> ReinitializeJobs()
    {
        await _hangfireConfig.InitializeRecurringJobsAsync();
        return Ok("Jobs reinitialized");
    }
}
```

### Example 4: Check Server Health
```csharp
// In Program.cs
if (app.IsHangfireHealthy())
{
    app.Logger.LogInformation("✓ Hangfire server is running");
}
else
{
    app.Logger.LogWarning("⚠ Hangfire server is NOT running");
}
```

## 🔐 Security

### Development Environment
- Full dashboard access ✓
- No authentication required
- Safe for testing

### Production Environment
- Requires Admin role for dashboard access
- Custom authorization filter: `HangfireAuthorizationFilter`
- Modify `Authorize()` method for custom logic:
```csharp
public bool Authorize(DashboardContext context)
{
    // Add your custom authorization logic here
    return context.GetHttpContext().User?.IsInRole("Admin") ?? false;
}
```

## 📈 Monitoring & Troubleshooting

### Check Server Status
```
Dashboard → Servers → View active Hangfire servers
```

### View Job Execution
```
Dashboard → Recurring Jobs → Click job name for history
```

### Debug Failed Jobs
```
Dashboard → Failed Jobs → Click job for full error details
```

### View Logs
- Application logs contain "Hangfire" entries
- Job execution logged to `ILogger<HangfireConfigurationService>`
- Failed job exceptions visible in dashboard

## ⚙️ Advanced Configuration

### Change Dashboard Path
```csharp
app.UseHangfireConfigured("/admin/jobs");  // Custom path
```

### Manual Job Scheduling
```csharp
var hangfireJobService = scope.ServiceProvider.GetRequiredService<IHangfireJobService>();

// Add custom recurring job
hangfireJobService.AddOrUpdateRecurring(
    "my-custom-job",
    () => MyService.DoWorkAsync(CancellationToken.None),
    Cron.Daily(2, 30)  // 2:30 AM daily
);
```

### Disable Dark Mode
```csharp
// In HangfireMiddlewareExtensions.cs, modify:
DarkModeEnabled = false  // Already set, can be toggled
```

## 🧪 Testing

### Verify Setup
1. Start application
2. Check logs for: "✓ Hangfire server is running"
3. Navigate to `/hangfire`
4. Verify 3 recurring jobs listed:
   - daily-revenue-calculation
   - monthly-revenue-summary
   - weekly-revenue-summary

### Manual Testing
1. Click on any recurring job
2. Click "Trigger now" button
3. Verify job runs immediately
4. Check "Succeeded" status in History

## 📚 Layer Architecture

```
Application Layer
├── IRevenueCalculationService (Business logic)
└── IHangfireConfigurationService (Configuration interface)
                ↓
Infrastructure Layer
├── HangfireConfigurationService (Config implementation)
├── HangfireJobService (Job scheduling)
├── RevenueCalculationService (Revenue business logic)
└── Extensions/HangfireMiddlewareExtensions.cs (Middleware)
                ↓
API Layer (Program.cs)
├── app.UseHangfireConfigured()
└── await app.InitializeHangfireJobsAsync()
                ↓
Hangfire Dashboard
└── /hangfire (UI & Monitoring)
```

## ✅ Verification

Build status:
```
dotnet build WebAppExam.API/WebAppExam.API.csproj
Result: Build succeeded ✓
Errors: 0
Warnings: ~10 (NuGet - non-critical)
```

All services properly:
- ✓ Registered in DependencyInjection
- ✓ Injected into configuration service
- ✓ Initialized on startup
- ✓ Available via dashboard
- ✓ Logged comprehensively
