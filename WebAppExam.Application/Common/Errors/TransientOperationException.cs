using System;

namespace WebAppExam.Infrastructure.Exceptions;

public class TransientOperationException : Exception
{
    public TransientOperationException(string message) : base(message) { }
    public TransientOperationException(string message, Exception innerException) : base(message, innerException) { }
}
