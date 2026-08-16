using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Confirmed,
    Preparing,
    OutForDelivery,
    Delivered,
    Cancelled
}
