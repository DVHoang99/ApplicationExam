using System;
using System.Diagnostics;
using System.Text;
using KafkaFlow.Producers;
using WebAppExam.Application.Logger.DTOs;

namespace WebAppExam.API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IProducerAccessor producerAccessor)
    {
        var stopwatch = Stopwatch.StartNew();
        string requestBody = string.Empty;

        if (context.Request.Method == HttpMethods.Post ||
            context.Request.Method == HttpMethods.Put ||
            context.Request.Method == HttpMethods.Patch)
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            requestBody = await reader.ReadToEndAsync();

            context.Request.Body.Position = 0;
        }

        try
        {
            await _next(context);

            stopwatch.Stop();
            var logProducer = producerAccessor.GetProducer("system-logs-producer");
            var logLevel = context.Response.StatusCode >= 400 ? "Warning" : "Info";

            var logContent = $"[{context.Request.Method}] {context.Request.Path}{context.Request.QueryString}";

            if (!string.IsNullOrEmpty(requestBody))
            {
                var truncatedBody = requestBody.Length > 2000 ? requestBody.Substring(0, 2000) + "... [TRUNCATED]" : requestBody;

                truncatedBody = truncatedBody.Replace("\r", "").Replace("\n", "");

                logContent += $" | Body: {truncatedBody}";
            }

            logContent += $" | Responded {context.Response.StatusCode} in {stopwatch.ElapsedMilliseconds}ms";

            var logMessage = new LogMessageDTO
            {
                Level = logLevel,
                ServiceName = "WebAppExam.Api",
                Message = logContent,
            };

            await logProducer.ProduceAsync(
                "system-logs-topic",
                Guid.NewGuid().ToString(),
                logMessage
            );
        }
        catch (Exception)
        {
            stopwatch.Stop();
            throw;
        }
    }
}
