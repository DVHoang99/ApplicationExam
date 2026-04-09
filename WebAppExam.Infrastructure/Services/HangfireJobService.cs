using System;
using System.Linq.Expressions;
using Hangfire;
using WebAppExam.Application.Services;

namespace WebAppExam.Infrastructure.Services;

/// <summary>
/// Hangfire background job service implementation
/// </summary>
public class HangfireJobService : IHangfireJobService, IJobService
{
    /// <summary>
    /// Enqueue a background job to run immediately (IJobService compatibility)
    /// </summary>
    public void Enqueue(Expression<Action> methodCall)
    {
        BackgroundJob.Enqueue(methodCall);
    }

    /// <summary>
    /// Enqueue a background job to run immediately
    /// </summary>
    string IHangfireJobService.Enqueue(Expression<Action> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }

    /// <summary>
    /// Enqueue an async background job to run immediately
    /// </summary>
    public string Enqueue(Expression<Func<Task>> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }

    /// <summary>
    /// Schedule a job to run after a specific delay
    /// </summary>
    public string Schedule(Expression<Action> methodCall, TimeSpan delay)
    {
        return BackgroundJob.Schedule(methodCall, delay);
    }

    /// <summary>
    /// Schedule an async job to run after a specific delay
    /// </summary>
    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        return BackgroundJob.Schedule(methodCall, delay);
    }

    /// <summary>
    /// Add or update a recurring job using cron expression
    /// </summary>
    /// <param name="recurringJobId">Unique identifier for the recurring job</param>
    /// <param name="methodCall">The method to execute</param>
    /// <param name="cronExpression">Cron expression (e.g., "0 0 * * *" for daily at midnight)</param>
    public void AddOrUpdateRecurring(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression);
    }

    /// <summary>
    /// Remove a recurring job
    /// </summary>
    public void RemoveIfExists(string recurringJobId)
    {
        RecurringJob.RemoveIfExists(recurringJobId);
    }

    /// <summary>
    /// Delete a background job
    /// </summary>
    public bool Delete(string jobId)
    {
        return BackgroundJob.Delete(jobId);
    }

    /// <summary>
    /// Requeue a job
    /// </summary>
    public bool Requeue(string jobId)
    {
        return BackgroundJob.Requeue(jobId);
    }
}
