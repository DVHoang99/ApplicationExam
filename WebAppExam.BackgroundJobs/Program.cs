using Hangfire;
using Hangfire.Redis.StackExchange;
using StackExchange.Redis;
using WebAppExam.Application;
using WebAppExam.BackgroundJobs.Services;
using WebAppExam.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add missing dependency for UnitOfWork since background jobs don't have HTTP contexts
builder.Services.AddScoped<WebAppExam.Application.Services.ICurrentUserService, WebAppExam.BackgroundJobs.Services.SystemCurrentUserService>();

 var hangfireDbConnection = builder.Configuration.GetSection("Redis")["HangfireDb"] ?? "localhost:6379,password=adminpassword,defaultDatabase=4";

var redis = ConnectionMultiplexer.Connect(hangfireDbConnection);

builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseRedisStorage(redis, new RedisStorageOptions
          {
              Prefix = "hangfire.job:",
              Db = 0
          });
});

builder.Services.AddHangfireServer();
builder.Services.AddScoped<IOutboxRetryJob, OutboxRetryJob>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseHangfireDashboard("/hangfire");

// 6. Đăng ký Recurring Job
RecurringJob.AddOrUpdate<IOutboxRetryJob>(
    "Outbox-Resend-Job",
    job => job.ExecuteAsync(),
    "* * * * *");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();

