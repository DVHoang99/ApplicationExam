using KafkaFlow.Producers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using WebAppExam.Application.Logger.DTOs;
using WebAppExam.Domain.Exceptions;

namespace WebAppExam.Infrastructure.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProducerAccessor _producerAccessor;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProducerAccessor producerAccessor, ILogger<GlobalExceptionHandler> logger)
    {
        _producerAccessor = producerAccessor;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Trace ID Correlation
        var traceId = httpContext.TraceIdentifier;

        // 2. Map Exception to Status and "Explanation"
        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException domainEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                domainEx.Message,
                domainEx.Errors),

            FluentValidation.ValidationException fluentEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                "One or more validation failures have occurred.",
                fluentEx.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray())),

            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                "Not Found",
                notFoundEx.Message,
                null),

            BadRequestException badRequestEx => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                badRequestEx.Message,
                null),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "You are not authorized to access this resource.",
                null),

            TransientOperationException transEx => (
                StatusCodes.Status400BadRequest,
                "Operation Failed",
                ExplainException(transEx),
                null),

            DbUpdateException dbEx => (
                StatusCodes.Status409Conflict,
                "Database Update Error",
                ExplainException(dbEx),
                null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please use the TraceId to report this issue.",
                null)
        };

        // 3. Log the Error with Trace ID (to Kafka and Serilog)
        await LogErrorAsync(httpContext, exception, traceId, statusCode, detail, errors);

        // 4. Return JSON Response to Client
        httpContext.Response.StatusCode = statusCode;
        
        // Extract flat error list for compatibility with ErrorResult format
        var flatErrors = new List<string>();
        if (errors is IDictionary<string, string[]> dict)
        {
            flatErrors = dict.SelectMany(x => x.Value).ToList();
        }
        else if (errors is IEnumerable<string> list)
        {
            flatErrors = list.ToList();
        }
        else
        {
            flatErrors.Add(detail ?? "An error occurred");
        }

        var isValidation = errors is IDictionary<string, string[]>;
        var response = new
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Errors = isValidation ? new List<string>() : flatErrors, 
            ValidationErrors = isValidation ? errors : null,
            TraceId = traceId
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private string ExplainException(Exception exception)
    {
        // Recursively look for inner exceptions of interest (like PostgresException)
        var current = exception;
        while (current != null)
        {
            if (current is PostgresException pgEx)
            {
                return pgEx.SqlState switch
                {
                    "23505" => "This record already exists (unique constraint violation).",
                    "23503" => "This operation violates a dependency constraint (foreign key violation).",
                    _ => $"Database error: {pgEx.MessageText}"
                };
            }
            current = current.InnerException;
        }

        return exception.Message;
    }

    private async Task LogErrorAsync(HttpContext context, Exception exception, string traceId, int statusCode, string detail, object? errors)
    {
        var level = statusCode >= 500 ? "Fatal" : "Warning";
        var logContext = new
        {
            TraceId = traceId,
            Method = context.Request.Method,
            Path = context.Request.Path,
            StatusCode = statusCode,
            Detail = detail,
            ValidationErrors = errors
        };

        _logger.Log(
            statusCode >= 500 ? LogLevel.Error : LogLevel.Warning,
            exception,
            "[{Level}] TraceId: {TraceId} | {Method} {Path} failed with Status {StatusCode}: {Detail} | ValidationErrors: {@ValidationErrors}",
            level, traceId, context.Request.Method, context.Request.Path, statusCode, detail, errors);

        try
        {
            var logProducer = _producerAccessor.GetProducer("system-logs-producer");
            
            var logMessage = LogMessageDTO.FromResult(
                level,
                "WebAppExam.Api",
                $"[{level}] TraceId: {traceId} | {context.Request.Method} {context.Request.Path} failed: {detail}",
                $"{exception}\n\nValidation Details: {System.Text.Json.JsonSerializer.Serialize(errors)}"
            );

            await logProducer.ProduceAsync(
                "system-logs-topic",
                Guid.NewGuid().ToString(),
                logMessage
            );
        }
        catch (Exception kafkaEx)
        {
            _logger.LogCritical(kafkaEx, "Failed to send error log to Kafka for TraceId: {TraceId}", traceId);
        }
    }
}