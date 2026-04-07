using System;

namespace WebAppExam.Application.Common.Errors;

public class ErrorResult(List<string> errors)
{
    public List<string> Errors { get; private set; } = errors;

    public static ErrorResult FromResult(List<string> errors)
    {
        return new ErrorResult(errors);
    }
}
