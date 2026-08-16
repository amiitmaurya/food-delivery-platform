using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Shared.Responses;

public class ValidationErrorResponse : ApiErrorResponse
{
    public Dictionary<string, string[]> ValidationErrors { get; set; } = [];
}