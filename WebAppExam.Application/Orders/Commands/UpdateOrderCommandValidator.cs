using System;
using FluentValidation;

namespace WebAppExam.Application.Orders.Commands;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public class OrderItemDtoValidator : FluentValidation.AbstractValidator<DTOs.OrderItemDto>
    {
        public OrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID cannot be empty.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        }
    }

    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId cannot be empty.");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemDtoValidator());
    }
}
