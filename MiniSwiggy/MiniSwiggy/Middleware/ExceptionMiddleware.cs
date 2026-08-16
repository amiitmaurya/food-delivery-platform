using System.Net;
using System.Text.Json;
using FluentValidation;
using MiniSwiggy.Shared.Exceptions;
using MiniSwiggy.Shared.Responses;

namespace MiniSwiggy.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);

            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse();

        switch (exception)
        {
            case ValidationException validationException:

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Validation Failed";
                response.Errors = validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                break;

            case BadRequestException ex:

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = ex.Message;

                break;

            case UnauthorizedException ex:

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Message = ex.Message;

                break;

            case ForbiddenException ex:

                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                response.StatusCode = StatusCodes.Status403Forbidden;
                response.Message = ex.Message;

                break;

            case NotFoundException ex:

                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = ex.Message;

                break;

            case ConflictException ex:

                context.Response.StatusCode = (int)HttpStatusCode.Conflict;

                response.StatusCode = StatusCodes.Status409Conflict;
                response.Message = ex.Message;

                break;

            case ArgumentException ex:

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = ex.Message;

                break;

            case InvalidOperationException ex:

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = ex.Message;

                break;

            case KeyNotFoundException ex:

                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = ex.Message;

                break;

            case Microsoft.EntityFrameworkCore.DbUpdateException ex:

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Database operation failed: " + (ex.InnerException?.Message ?? ex.Message);

                break;

            default:

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = !string.IsNullOrWhiteSpace(exception.Message) ? exception.Message : "An unexpected error occurred.";

                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var result = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(result);
    }
}