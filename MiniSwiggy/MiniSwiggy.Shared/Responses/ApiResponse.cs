using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Shared.Responses;

public class ApiResponse<T>
{
    public bool Success { get; init; }

    public int StatusCode { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public ApiResponse()
    {
    }

    public ApiResponse(bool success, int statusCode, string message, T? data = default)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }
}
