using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiniSwiggy.Shared.Responses;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace MiniSwiggy.API.Middleware;

public class CustomAutoValidationResultFactory : IFluentValidationAutoValidationResultFactory
{
    public Task<IActionResult?> CreateActionResult(
        ActionExecutingContext context,
        ValidationProblemDetails? validationProblemDetails,
        IDictionary<IValidationContext, ValidationResult>? validationResults)
    {
        var errors = new Dictionary<string, string[]>();

        if (validationResults != null && validationResults.Count > 0)
        {
            foreach (var kvp in validationResults)
            {
                var valResult = kvp.Value;
                if (valResult?.Errors != null && valResult.Errors.Count > 0)
                {
                    var grouped = valResult.Errors
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray());

                    foreach (var g in grouped)
                    {
                        errors[g.Key] = g.Value;
                    }
                }
            }
        }

        if (errors.Count == 0 && validationProblemDetails?.Errors != null && validationProblemDetails.Errors.Count > 0)
        {
            foreach (var error in validationProblemDetails.Errors)
            {
                errors[error.Key] = error.Value;
            }
        }

        if (errors.Count == 0 && !context.ModelState.IsValid)
        {
            foreach (var state in context.ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    errors[state.Key] = state.Value.Errors.Select(e => e.ErrorMessage).ToArray();
                }
            }
        }

        var response = new ApiErrorResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Validation Failed",
            Errors = errors.Count > 0 ? errors : null
        };

        IActionResult? result = new BadRequestObjectResult(response);
        return Task.FromResult<IActionResult?>(result);
    }
}
