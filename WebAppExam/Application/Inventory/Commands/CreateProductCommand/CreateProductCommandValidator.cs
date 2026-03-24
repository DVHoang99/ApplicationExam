using FluentValidation;

namespace WebAppExam.Application.Inventory.Command.CreateProductCommand
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description is too long.");

            RuleFor(x => x.Quantity).GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        }
    }
}