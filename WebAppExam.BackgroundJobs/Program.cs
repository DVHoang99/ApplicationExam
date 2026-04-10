using Hangfire;
using Hangfire.Redis.StackExchange;
using StackExchange.Redis;
using WebAppExam.Application;
using WebAppExam.BackgroundJobs.Services;
using WebAppExam.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var redisConnectionString = builder.Configuration.GetValue<string>("RedisConnection");

var redis = ConnectionMultiplexer.Connect(redisConnectionString);

// 3. Cấu hình Hangfire sử dụng Redis Storage
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
    Cron.Hourly);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
