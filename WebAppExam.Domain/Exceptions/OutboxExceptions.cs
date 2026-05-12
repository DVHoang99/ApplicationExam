using System;

namespace WebAppExam.Domain.Exceptions;

/// <summary>
/// Exception thrown for transient errors during outbox publication that should be retried.
/// </summary>
public class TransientOutboxException : Exception
{
    public TransientOutboxException(string message) : base(message) { }
    public TransientOutboxException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown for permanent errors during outbox publication that should stop retrying.
/// </summary>
public class PermanentOutboxException : Exception
{
    public PermanentOutboxException(string message) : base(message) { }
    public PermanentOutboxException(string message, Exception innerException) : base(message, innerException) { }
}
