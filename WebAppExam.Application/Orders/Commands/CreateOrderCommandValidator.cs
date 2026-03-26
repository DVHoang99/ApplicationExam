using System;
using FluentValidation;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Commands;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
    {
        public OrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID cannot be empty.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        }
    }

    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage(x => $"CustomerId {x.CustomerId} cannot be empty.");

        RuleForEach(x => x.Items)
                .SetValidator(new OrderItemDtoValidator());
    }
}
