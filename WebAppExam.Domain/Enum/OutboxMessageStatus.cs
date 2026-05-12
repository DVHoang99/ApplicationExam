namespace WebAppExam.Domain.Enum;

public enum OutboxMessageStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Completed = 3
}
