using System;

namespace WebAppExam.Application.Logger.DTOs;

public class AuditLogMessageDTO
{
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Create, Update, Delete
    public string PrimaryKey { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ChangedBy { get; set; } = string.Empty;

}
