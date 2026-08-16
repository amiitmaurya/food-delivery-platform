using FluentValidation;
using MiniSwiggy.Application.DTOs.Wishlist;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Validators.Wishlist;

public class AddToWishlistRequestValidator : AbstractValidator<AddToWishlistRequest>
{
    public AddToWishlistRequestValidator()
    {
        RuleFor(x => x.FoodItemId)
            .GreaterThan(0)
            .WithMessage("Invalid food item.");
    }
}
