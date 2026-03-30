using System;
using System.Data;
using FluentValidation;

namespace WebAppExam.Application.Auth.Commands;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(x => $"Username {x.Username} cannot be empty.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(x => $"Password {x.Password} cannot be empty.");
    }
}
