using System;
using FluentValidation;

namespace WebAppExam.Application.Products.Commands;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");

        RuleForEach(x => x.Inventories).ChildRules(inventory =>
        {
            inventory.RuleFor(i => i.Name)
                .NotEmpty().WithMessage("Inventory name is required.")
                .MaximumLength(100).WithMessage("Inventory name must not exceed 100 characters.");
            inventory.RuleFor(i => i.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.");
        });

    }
}