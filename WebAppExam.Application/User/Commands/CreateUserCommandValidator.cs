using System;
using FluentValidation;

namespace WebAppExam.Application.User.Commands;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(x => $"Username {x.Username} cannot be empty.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(x => $"Password {x.Password} cannot be empty.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(x => $"Name {x.Name} cannot be empty.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage(x => $"Role {x.Role} cannot be empty.");
    }
}
