using System;

namespace WebAppExam.Domain.Entity;

public class InboxMessage : EntityBase
{
    public string MessageId { get; private set; }
    public string Type { get; private set; }
    public string? Content { get; private set; }
    public string? Status { get; private set; }
    public string? Error { get; private set; }

    private InboxMessage() { }

    private InboxMessage(Ulid id, string messageId, string type, string status, string? content = null)
    {
        Id = id;
        MessageId = messageId;
        Type = type;
        Content = content;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public static InboxMessage Create(string messageId, string type, string status, string? content = null)
    {
        return new InboxMessage(Ulid.NewUlid(), messageId, type, status, content);
    }

    public void MarkAsProcessed()
    {
        Status = "Processed";
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Status = "Failed";
        Error = error;
        UpdatedAt = DateTime.UtcNow;
    }
}
