using System;
using System.Linq;
using System.Text.Json;
using Hangfire.Common;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebAppExam.Application.OutboxMessages;
using WebAppExam.Domain.Enum;
using WebAppExam.Domain.Exceptions;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Infrastructure.Common.Hangfire;

/// <summary>
/// A Hangfire Server Filter that acts as a "Handler" for Outbox message publication.
/// It intercepts the completion of the PublishMessageAsync job and updates the 
/// outbox record status in the database based on the success or failure of the job.
/// </summary>
public class OutboxJobFilter : JobFilterAttribute, IServerFilter
{
    public void OnPerforming(PerformingContext filterContext)
    {
        // No logic needed before execution
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        // Only target the PublishMessageAsync method
        if (filterContext.BackgroundJob.Job.Method.Name != "PublishMessageAsync")
        {
            return;
        }

        // Extract outboxMessageId from the first argument
        if (filterContext.BackgroundJob.Job.Args.FirstOrDefault() is not Ulid outboxMessageId)
        {
            return;
        }

        // In ASP.NET Core integration, the scope is available in the Items dictionary.
        // We'll try to find any IServiceProvider or IServiceScope in the items.
        var serviceScope = filterContext.Items.Values.OfType<IServiceScope>().FirstOrDefault();
        if (serviceScope == null)
        {
            return;
        }

        var outboxRepository = serviceScope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<OutboxJobFilter>>();

        if (filterContext.Exception == null || filterContext.ExceptionHandled)
        {
            // SUCCESS HANDLER
            outboxRepository.UpdateStatusAsync(outboxMessageId, OutboxMessageStatus.Sent).GetAwaiter().GetResult();
            
            logger.LogInformation("Outbox Job Handler: Successfully marked message {Id} as Sent.", outboxMessageId);
        }
        else
        {
            // FAILURE HANDLER
            var exception = filterContext.Exception;
            bool isPermanent = exception is PermanentOutboxException || exception is JsonException || exception is ArgumentException;

            if (isPermanent)
            {
                outboxRepository.UpdateStatusAsync(
                    outboxMessageId, 
                    OutboxMessageStatus.Failed, 
                    exception.Message, 
                    isPermanentFailure: true
                ).GetAwaiter().GetResult();

                logger.LogCritical(exception, "Outbox Job Handler: PERMANENT FAILURE for message {Id}.", outboxMessageId);
            }
            else
            {
                // Note: We don't increment retry count here because Hangfire's AutomaticRetry will re-enqueue.
                // We just record the last error.
                outboxRepository.UpdateStatusAsync(
                    outboxMessageId, 
                    OutboxMessageStatus.Pending, 
                    exception.Message, 
                    isPermanentFailure: false
                ).GetAwaiter().GetResult();

                logger.LogWarning("Outbox Job Handler: TRANSIENT FAILURE recorded for message {Id}. Error: {Msg}", 
                    outboxMessageId, exception.Message);
            }
        }
    }
}
