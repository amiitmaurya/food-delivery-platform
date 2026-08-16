using FluentValidation;
using MiniSwiggy.Application.DTOs.Coupon;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Validators.Coupon;

public class CreateCouponRequestValidator : AbstractValidator<CreateCouponRequest>
{
    public CreateCouponRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Coupon code is required.")
            .Length(3, 30)
            .WithMessage("Coupon code must be between 3 and 30 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.DiscountType)
            .NotEmpty()
            .Must(x => x.Equals("Flat", StringComparison.OrdinalIgnoreCase)
                    || x.Equals("Percentage", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Discount type must be Flat or Percentage.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .WithMessage("Discount value must be greater than zero.");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum order amount cannot be negative.");

        RuleFor(x => x.MaximumDiscount)
            .GreaterThan(0)
            .When(x => x.MaximumDiscount.HasValue)
            .WithMessage("Maximum discount must be greater than zero.");

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("Expiry date must be after start date.");

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .WithMessage("Usage limit must be greater than zero.");
    }
}
