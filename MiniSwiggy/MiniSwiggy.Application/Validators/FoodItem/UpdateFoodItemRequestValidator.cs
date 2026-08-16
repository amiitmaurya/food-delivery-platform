using FluentValidation;
using MiniSwiggy.Application.DTOs.FoodItem;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Validators.FoodItem;

public class UpdateFoodItemRequestValidator : AbstractValidator<UpdateFoodItemRequest>
{
    public UpdateFoodItemRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Invalid food item id.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Category is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Food item name is required.")
            .Length(2, 100)
            .WithMessage("Food item name must be between 2 and 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero.");

        RuleFor(x => x.OfferPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.OfferPrice.HasValue)
            .WithMessage("Offer price cannot be negative.");

        RuleFor(x => x)
            .Must(x => !x.OfferPrice.HasValue || x.OfferPrice.Value <= x.Price)
            .WithMessage("Offer price cannot be greater than actual price.");

        RuleFor(x => x.Image)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Image));
    }
}
