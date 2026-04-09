using System;
using System.Linq.Expressions;

namespace WebAppExam.Application.Services;

/// <summary>
/// Interface for Hangfire background job scheduling
/// </summary>
public interface IHangfireJobService
{
    /// <summary>
    /// Enqueue a background job
    /// </summary>
    string Enqueue(Expression<Action> methodCall);

    /// <summary>
    /// Enqueue an async background job
    /// </summary>
    string Enqueue(Expression<Func<Task>> methodCall);

    /// <summary>
    /// Schedule a job to run at a specific time
    /// </summary>
    string Schedule(Expression<Action> methodCall, TimeSpan delay);

    /// <summary>
    /// Schedule an async job to run at a specific time
    /// </summary>
    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);

    /// <summary>
    /// Schedule a recurring job (Cron expression)
    /// </summary>
    void AddOrUpdateRecurring(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression);

    /// <summary>
    /// Remove a recurring job
    /// </summary>
    void RemoveIfExists(string recurringJobId);

    /// <summary>
    /// Delete a job by ID
    /// </summary>
    bool Delete(string jobId);

    /// <summary>
    /// Requeue a job
    /// </summary>
    bool Requeue(string jobId);
}
