using FluentResults;

namespace WebAppExam.Application.Common.Errors;

public class NotFoundError : Error
{
    public NotFoundError(string resource, string identifier)
        : base($"{resource} with identifier '{identifier}' not found")
    {
        Metadata.Add("ResourceType", resource);
        Metadata.Add("Identifier", identifier);
    }

    public static NotFoundError Create(string resourceName)
    {
        return new NotFoundError(resourceName, "");
    }
}
