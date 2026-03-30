using System;
using FluentValidation;

namespace WebAppExam.Application.Auth.Commands;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(x => $"Token cannot be empty.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(x => $"RefreshToken cannot be empty.");
    }
}
