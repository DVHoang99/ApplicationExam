using FluentResults;

namespace WebAppExam.Application.Common.Errors;

public class ValidationError : Error
{
    public Dictionary<string, string[]> ErrorMessages { get; }

    public ValidationError(Dictionary<string, string[]> errorMessages)
        : base("Validation failed")
    {
        ErrorMessages = errorMessages;
        Metadata.Add("ValidationErrors", errorMessages);
    }

    public static ValidationError FromFluentValidation(FluentValidation.Results.ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

        return new ValidationError(errors);
    }
}
