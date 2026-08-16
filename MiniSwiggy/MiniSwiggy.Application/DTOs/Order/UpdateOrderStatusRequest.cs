using MiniSwiggy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Order;

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}