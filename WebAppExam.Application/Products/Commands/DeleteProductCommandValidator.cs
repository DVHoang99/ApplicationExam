using System;
using FluentValidation;

namespace WebAppExam.Application.Products.Commands;

public class DeleteProductCommandValidator : FluentValidation.AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");
    }
}
