using FluentValidation;
using MiniSwiggy.Application.DTOs.Address;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Validators.Address;

public class UpdateAddressRequestValidator : AbstractValidator<UpdateAddressRequest>
{
    public UpdateAddressRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required.")
            .Length(3, 100)
            .WithMessage("Full name must be between 3 and 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(@"^(\+91)?[6-9]\d{9}$")
            .WithMessage("Enter a valid 10-digit mobile number.");

        RuleFor(x => x.HouseNo)
            .NotEmpty()
            .WithMessage("House number is required.")
            .MaximumLength(100);

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street is required.")
            .MaximumLength(200);

        RuleFor(x => x.Landmark)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Landmark));

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required.")
            .MaximumLength(100);

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State is required.")
            .MaximumLength(100);

        RuleFor(x => x.Pincode)
            .NotEmpty()
            .WithMessage("Pincode is required.")
            .Matches(@"^\d{6}$")
            .WithMessage("Pincode must be a valid 6-digit number.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180.");
    }
}
