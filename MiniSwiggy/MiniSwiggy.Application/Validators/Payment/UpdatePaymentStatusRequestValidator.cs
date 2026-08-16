using FluentValidation;
using MiniSwiggy.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Validators.Payment;

public class UpdatePaymentStatusRequestValidator : AbstractValidator<UpdatePaymentStatusRequest>
{
    public UpdatePaymentStatusRequestValidator()
    {
        RuleFor(x => x.PaymentId)
            .GreaterThan(0)
            .WithMessage("Invalid payment id.");

        RuleFor(x => x.PaymentStatus)
            .IsInEnum()
            .WithMessage("Invalid payment status.");

        RuleFor(x => x.TransactionId)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.TransactionId));

        RuleFor(x => x.GatewayOrderId)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.GatewayOrderId));
    }
}

