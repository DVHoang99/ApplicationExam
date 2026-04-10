using System;
using System.Net;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Domain.Entity;

public class OutboxMessage
{
    public Ulid Id { get; set; }
    public string Type { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
    public OutboxMessageStatus Status { get; set; }
    public string MessageId { get; set; }

    private OutboxMessage(Ulid id, string type, string content, DateTime createdAt, OutboxMessageStatus status, string messageId)
    {
        Id = id;
        Type = type;
        Content = content;
        CreatedAt = createdAt;
        Status = status;
        MessageId = messageId;
    }

    public static OutboxMessage Init(Ulid id, string type, string content, string messageId)
    {
        return new OutboxMessage(
            id: id,
            type: type,
            content: content,
            createdAt: DateTime.UtcNow,
            status: OutboxMessageStatus.Pending,
            messageId: messageId
        );
    }

    public void UpdateStatus(OutboxMessageStatus status, string error)
    {
        Status = status;
        ProcessedOn = DateTime.UtcNow;
        Error = error;
    }
}

