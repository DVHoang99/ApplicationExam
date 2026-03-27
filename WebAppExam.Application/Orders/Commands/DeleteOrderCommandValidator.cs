using FluentValidation;

namespace WebAppExam.Application.Orders.Commands;

public class DeleteOrderCommandValidator : FluentValidation.AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order ID is required.");
    }
}

