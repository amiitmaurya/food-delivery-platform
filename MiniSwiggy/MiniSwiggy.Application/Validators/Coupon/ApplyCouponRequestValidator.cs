using FluentValidation;
using MiniSwiggy.Application.DTOs.Coupon;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Validators.Coupon;

public class ApplyCouponRequestValidator : AbstractValidator<ApplyCouponRequest>
{
    public ApplyCouponRequestValidator()
    {
        RuleFor(x => x.CouponCode)
            .NotEmpty()
            .WithMessage("Coupon code is required.")
            .Length(3, 30)
            .WithMessage("Coupon code must be between 3 and 30 characters.");

        RuleFor(x => x.CartTotal)
            .GreaterThan(0)
            .WithMessage("Cart total must be greater than zero.");
    }
}
