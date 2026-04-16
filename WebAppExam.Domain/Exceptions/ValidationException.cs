using System.Collections.Generic;
using System.Linq;

namespace WebAppExam.Domain.Exceptions;

public class ValidationException : DomainException
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<string> failures)
        : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { "General", failures.ToArray() }
        };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : this()
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}
