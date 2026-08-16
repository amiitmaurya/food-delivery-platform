using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.Order;
using MiniSwiggy.Application.Interfaces;
using System.Security.Claims;

namespace MiniSwiggy.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private int GetUserId()
    {
        var claim = User?.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return userId;
    }

    // Place Order
    [HttpPost("place")]
    public async Task<IActionResult> PlaceOrder(PlaceOrderRequest request)
    {
        var result = await _orderService.PlaceOrderAsync(GetUserId(), request);

        return Ok(result);
    }

    // My Orders
    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        var result = await _orderService.GetMyOrdersAsync(GetUserId());

        return Ok(result ?? new List<OrderSummaryResponse>());
    }

    // All Platform Orders (Admin & SuperAdmin)
    [HttpGet("all")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAllOrders()
    {
        var result = await _orderService.GetAllOrdersAsync();

        return Ok(result ?? new List<OrderSummaryResponse>());
    }

    // Order Details
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var result = await _orderService.GetOrderByIdAsync(id, GetUserId());

        if (result == null)
            return NotFound(new { message = "Order not found." });

        return Ok(result);
    }

    // Cancel Order
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var result = await _orderService.CancelOrderAsync(id, GetUserId());

        if (!result)
            return BadRequest(new { message = "Unable to cancel order." });

        return Ok(new { message = "Order cancelled successfully." });
    }

    // Update Order Status (Admin & DeliveryPartner)
    [HttpPut("{id}/status")]
    [Authorize(Roles = "SuperAdmin,Admin,DeliveryPartner")]
    public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest request)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, request);

        if (!result)
            return BadRequest(new { message = "Unable to update status." });

        return Ok(new { message = "Order status updated successfully." });
    }
}