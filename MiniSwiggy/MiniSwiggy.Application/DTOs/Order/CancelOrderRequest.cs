using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Order;

public class CancelOrderRequest
{
    public string? Reason { get; set; }
}
