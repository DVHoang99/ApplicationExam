using System;

namespace WebAppExam.Application.Logger.DTOs;

public class AuditLogMessageDTO
{
    public string EntityName { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty; // Create, Update, Delete
    public string PrimaryKey { get; private set; } = string.Empty;
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
    public string ChangedBy { get; private set; } = string.Empty;

    public static AuditLogMessageDTO FromResult(string entityName, string action, string? primaryKey, string? oldValues, string? newValues)
    {
        return new AuditLogMessageDTO
        {
            EntityName = entityName,
            Action = action,
            PrimaryKey = primaryKey ?? "",
            OldValues = oldValues,
            NewValues = newValues,
        };
    }
}
