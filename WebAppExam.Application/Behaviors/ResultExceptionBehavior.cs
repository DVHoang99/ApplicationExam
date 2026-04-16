using FluentResults;
using MediatR;
using WebAppExam.Application.Common.Errors;
using WebAppExam.Domain.Exceptions;

namespace WebAppExam.Application.Behaviors;

/// <summary>
/// This behavior intercepts responses of type IResultBase (from FluentResults).
/// If the result is a failure, it throws a corresponding DomainException 
/// which will be caught by the GlobalExceptionHandler middleware.
/// </summary>
public class ResultExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (response is IResultBase result && result.IsFailed)
        {
            var errors = result.Errors;
            var firstErrorMessage = errors.FirstOrDefault()?.Message ?? "An operation failed.";

            // 1. Structured Validation Errors
            var validationErrors = errors.OfType<ValidationError>().ToList();
            if (validationErrors.Any())
            {
                var allErrors = validationErrors.SelectMany(ve => ve.ErrorMessages)
                    .GroupBy(x => x.Key)
                    .ToDictionary(g => g.Key, g => g.SelectMany(x => x.Value).ToArray());
                
                throw new ValidationException(allErrors);
            }

            // 2. Not Found Errors
            if (errors.Any(e => e is NotFoundError))
            {
                throw new NotFoundException(firstErrorMessage);
            }

            // 3. Unauthorized Errors
            if (errors.Any(e => e is UnauthorizedError))
            {
                throw new UnauthorizedAccessException(firstErrorMessage);
            }

            // 4. Fallback to BadRequest for most business errors
            throw new BadRequestException(firstErrorMessage);
        }

        return response;
    }
}
