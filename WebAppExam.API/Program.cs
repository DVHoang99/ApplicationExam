using System.Security.Claims;
using System.Text;
using Hangfire;
using KafkaFlow;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using WebAppExam.Application;
using WebAppExam.Infrastructure;
using WebAppExam.API.Services;
using WebAppExam.Infrastructure.Exceptions;
using WebAppExam.Infrastructure.Jobs;
using WebAppExam.Domain.Common;
using WebAppExam.Application.Services;
using Serilog;
using WebAppExam.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration));

// 1. Register Layer Dependencies
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. API Specific Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddGrpc();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            throw new WebAppExam.Domain.Exceptions.ValidationException(errors);
        };
    });

// 3. Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAssertion(context =>
        {
            var httpContext = context.Resource as HttpContext;
            if (httpContext == null) return false;

            if (httpContext.User.Identity?.IsAuthenticated == true) return true;

            if (httpContext.Request.Headers.TryGetValue(Constants.HttpHeader.InternalKeyHeader, out var extractedKey))
            {
                var secretKey = builder.Configuration[Constants.ConfigKeys.InternalApiKeyConfigPath];
                return !string.IsNullOrEmpty(secretKey) && extractedKey == secretKey;
            }

            return false;
        })
        .Build();
});

var app = builder.Build();

// 4. Configure Hangfire Recurring Jobs
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<RevenueBackgroundJob>(
        Constants.HangfireJob.DailyRevenueCalculation,
        job => job.RunDailyCalculation(),
        "59 23 * * *"
    );
}

// 5. Middleware Pipeline
app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<OutboxGrpcService>();

// 6. Start Kafka
var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.Run();