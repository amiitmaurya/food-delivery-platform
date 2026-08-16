using FluentValidation;
using MiniSwiggy.Application.DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Validators.Cart;

public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartRequestValidator()
    {
        RuleFor(x => x.FoodItemId)
            .GreaterThan(0)
            .WithMessage("Invalid food item.");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 20)
            .WithMessage("Quantity must be between 1 and 20.");
    }
}

