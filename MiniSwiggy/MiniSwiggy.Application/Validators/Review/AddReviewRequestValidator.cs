using FluentValidation;
using MiniSwiggy.Application.DTOs.Review;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Validators.Review;

public class AddReviewRequestValidator : AbstractValidator<AddReviewRequest>
{
    public AddReviewRequestValidator()
    {
        RuleFor(x => x.FoodItemId)
            .GreaterThan(0)
            .WithMessage("Invalid food item.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(500)
            .WithMessage("Comment cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Comment));
    }
}
