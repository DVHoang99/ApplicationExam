using System;

namespace WebAppExam.Application.Logger.DTOs;

public class LogMessageDTO
{
    public string Level { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
