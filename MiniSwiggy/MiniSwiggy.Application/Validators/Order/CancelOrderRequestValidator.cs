using FluentValidation;
using MiniSwiggy.Application.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Validators.Order;

public class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(300)
            .WithMessage("Cancellation reason cannot exceed 300 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }
}
