using System;

namespace WebAppExam.Domain.Entity;

public class AuditLogEntry
{
    public string Id { get; set; }
    public string EntityName { get; set; }
    public string Action { get; set; }
    public string PrimaryKey { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string ChangedBy { get; set; }
    public DateTime Timestamp { get; set; }


    public AuditLogEntry(
        string entityName,
        string action,
        string primaryKey,
        string? oldValues,
        string? newValues,
        string changedBy)
    {
        Id = Ulid.NewUlid().ToString();
        EntityName = entityName;
        Action = action;
        PrimaryKey = primaryKey;
        OldValues = oldValues;
        NewValues = newValues;
        ChangedBy = changedBy;
        Timestamp = DateTime.UtcNow;
    }
}
