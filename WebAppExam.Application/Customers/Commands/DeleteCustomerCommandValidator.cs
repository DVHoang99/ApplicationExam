using System;
using FluentValidation;

namespace WebAppExam.Application.Customers.Commands;

public class DeleteCustomerCommandValidator : FluentValidation.AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}
