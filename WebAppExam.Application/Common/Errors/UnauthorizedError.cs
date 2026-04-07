using FluentResults;

namespace WebAppExam.Application.Common.Errors;

public class UnauthorizedError : Error
{
    public UnauthorizedError(string message) : base(message)
    {
    }

    public static UnauthorizedError Unauthorized(string message)
    {
        return new UnauthorizedError(message);
    }
}
