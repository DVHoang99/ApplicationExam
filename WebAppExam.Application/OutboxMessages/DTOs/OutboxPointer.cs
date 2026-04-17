using System;

namespace WebAppExam.Application.OutboxMessages.DTOs;

/// <summary>
/// A lightweight pointer to an outbox message.
/// Used for the Claim Check pattern where the Kafka message only contains the ID.
/// </summary>
public class OutboxPointer
{
    public string Id { get; set; }
    public string Type { get; set; }

    public OutboxPointer(string id, string type)
    {
        Id = id;
        Type = type;
    }

    public OutboxPointer() { } // For deserialization
}
