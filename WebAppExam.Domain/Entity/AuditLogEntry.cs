using System;

namespace WebAppExam.Domain.Entity;

public class AuditLogEntry
{
    public string Id { get; private set; }
    public string EntityName { get; private set; }
    public string Action { get; private set; }
    public string PrimaryKey { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string ChangedBy { get; private set; }
    public DateTime Timestamp { get; private set; }


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
