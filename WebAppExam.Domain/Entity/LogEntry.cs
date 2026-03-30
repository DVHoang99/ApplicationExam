using System;

namespace WebAppExam.Domain.Entity;

public class LogEntry
{
    public string Id { get; private set; }
    public string Level { get; private set; }
    public string ServiceName { get; private set; }
    public string Message { get; private set; }
    public string? Exception { get; private set; }
    public DateTime Timestamp { get; private set; }

    public LogEntry(string level, string serviceName, string message, string? exception)
    {
        Id = Ulid.NewUlid().ToString();
        Level = level;
        ServiceName = serviceName;
        Message = message;
        Exception = exception;
        Timestamp = DateTime.UtcNow;
    }
}
