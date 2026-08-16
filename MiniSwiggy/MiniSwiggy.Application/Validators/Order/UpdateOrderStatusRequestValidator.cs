using FluentValidation;
using MiniSwiggy.Application.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Validators.Order;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid order status.");
    }
}
