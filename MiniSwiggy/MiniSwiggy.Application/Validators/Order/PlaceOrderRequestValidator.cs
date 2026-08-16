using FluentValidation;
using MiniSwiggy.Application.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Validators.Order;

public class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderRequestValidator()
    {
        RuleFor(x => x.DeliveryAddress)
            .NotEmpty()
            .WithMessage("Delivery address is required.")
            .MaximumLength(500)
            .WithMessage("Delivery address cannot exceed 500 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(@"^(\+91)?[6-9]\d{9}$")
            .WithMessage("Enter a valid 10-digit mobile number.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("Invalid payment method.");
    }
}
