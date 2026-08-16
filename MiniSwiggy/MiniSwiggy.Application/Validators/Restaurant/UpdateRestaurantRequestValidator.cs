using FluentValidation;
using MiniSwiggy.Application.DTOs.Restaurant;

namespace MiniSwiggy.Application.Validators.Restaurant;

public class UpdateRestaurantRequestValidator : AbstractValidator<UpdateRestaurantRequest>
{
    public UpdateRestaurantRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Invalid restaurant id.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Restaurant name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(300);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.OwnerName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.OwnerName));

        RuleFor(x => x.MobileNumber)
            .Matches(@"^(\+91)?[6-9]\d{9}$")
            .WithMessage("Enter a valid 10-digit mobile number.")
            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber));

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Enter a valid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.State)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.State));

        RuleFor(x => x.Pincode)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.Pincode));

        RuleFor(x => x.DeliveryTime)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DeliveryCharge)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AverageCostForTwo)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.OpeningTime == TimeSpan.Zero || x.ClosingTime == TimeSpan.Zero || x.OpeningTime < x.ClosingTime)
            .WithMessage("Opening time must be earlier than closing time.");
    }
}
