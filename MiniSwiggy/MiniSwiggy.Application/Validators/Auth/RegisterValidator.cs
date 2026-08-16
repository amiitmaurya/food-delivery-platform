using FluentValidation;
using MiniSwiggy.Application.DTOs.Auth;

namespace MiniSwiggy.Application.Validators.Aurh_Validator;

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^(\+91)?[6-9]\d{9}$")
            .WithMessage("Enter a valid 10-digit mobile number.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .WithMessage("Password must contain one uppercase letter.")
            .Matches("[a-z]")
            .WithMessage("Password must contain one lowercase letter.")
            .Matches("[0-9]")
            .WithMessage("Password must contain one number.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must contain one special character.");


    }
}