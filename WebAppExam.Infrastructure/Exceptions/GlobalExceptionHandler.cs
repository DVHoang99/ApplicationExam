using System;
using KafkaFlow.Producers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebAppExam.Application.Logger.DTOs;

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
        if (exception is FluentValidation.ValidationException validationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            httpContext.Response.ContentType = "application/json";

            var validationResponse = new
            {
                Status = 400,
                Title = "Validation Error",
                Errors = validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray())
            };

            await httpContext.Response.WriteAsJsonAsync(validationResponse, cancellationToken);
            return true;
        }

        _logger.LogError(exception, "A completely unhandled exception occurred.");

        try
        {
            var logProducer = _producerAccessor.GetProducer("system-logs-producer");
            var logMessage = new LogMessageDTO
            {
                Level = "Fatal",
                ServiceName = "WebAppExam.Api",
                Message = $"[CRASH] {httpContext.Request.Method} {httpContext.Request.Path} failed: {exception.Message}",
                Exception = exception.ToString(),
            };

            await logProducer.ProduceAsync(
                "system-logs-topic",
                Guid.NewGuid().ToString(),
                logMessage
            );
        }
        catch (Exception kafkaEx)
        {
            _logger.LogCritical(kafkaEx, "Failed to send crash log to Kafka!");
        }

        // Trả về JSON lỗi 500 cho Frontend thay vì văng nguyên trang HTML
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var serverErrorResponse = new
        {
            Status = 500,
            Title = "An unexpected server error occurred.",
            Detail = exception.Message
        };

        await httpContext.Response.WriteAsJsonAsync(serverErrorResponse, cancellationToken);
        return true;
    }
}