using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Shared.Responses;

public class ApiErrorResponse
{
    public bool Success => false;

    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public object? Errors { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
