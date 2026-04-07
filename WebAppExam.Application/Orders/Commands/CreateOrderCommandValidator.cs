using System;
using System.Data;
using FluentValidation;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.Application.Orders.Commands;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
        public class OrderItemDtoValidator : AbstractValidator<OrderItemDTO>
        {
                public OrderItemDtoValidator()
                {
                        RuleFor(x => x.ProductId)
                            .NotEmpty().WithMessage("Product ID cannot be empty.");

                        RuleFor(x => x.Quantity)
                            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

                        RuleFor(x => x.WareHouseId)
                                .NotEmpty().WithMessage("Product ID cannot be empty.");
                }
        }

        public CreateOrderCommandValidator()
        {
                RuleFor(x => x.CustomerId)
                        .NotEmpty().WithMessage(x => $"CustomerId {x.CustomerId} cannot be empty.");
                RuleFor(x => x.CustomerName)
                        .NotEmpty().WithMessage(x => $"CustomerName {x.CustomerName} cannot be empty.");
                RuleFor(x => x.Address)
                        .NotEmpty().WithMessage(x => $"Address {x.Address} cannot be empty.")
                        .MaximumLength(200).WithMessage("Address must not exceed 100 characters.");
                RuleFor(x => x.PhoneNumber)
                        .NotEmpty().WithMessage(x => $"PhoneNumber {x.PhoneNumber} cannot be empty.")
                        .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number is not valid.");

                RuleForEach(x => x.Items)
                        .SetValidator(new OrderItemDtoValidator());
        }
}
