using System;

namespace WebAppExam.Application.Logger.DTOs;

public class LogMessageDTO
{
    public string Level { get; private set; } = string.Empty;
    public string ServiceName { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? Exception { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    public static LogMessageDTO FromResult(string level, string serviceName, string message, string? exception)
    {
        return new LogMessageDTO
        {
            Level = level,
            ServiceName = serviceName,
            Message = message,
            Exception = exception,  
        };
    }
}
