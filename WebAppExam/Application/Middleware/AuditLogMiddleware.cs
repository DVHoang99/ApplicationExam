using MassTransit;
using System.Diagnostics;
using System.Text;
using WebAppExam.Domain.LogViewModel;

namespace WebAppExam.Application.Middleware
{
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IPublishEndpoint publishEndpoint)
        {
            var stopwatch = Stopwatch.StartNew();

            var request = context.Request;

            // Read body
            request.EnableBuffering();
            string body = "";

            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            await _next(context);

            stopwatch.Stop();

            var auditEvent = new AuditLogEvent
            {
                TraceId = context.TraceIdentifier,
                Method = request.Method,
                Path = request.Path,
                StatusCode = context.Response.StatusCode,
                Duration = stopwatch.ElapsedMilliseconds,
                RequestBody = body,
                CreatedAt = DateTime.UtcNow
            };

            await publishEndpoint.Publish(auditEvent);
        }
    }
}
