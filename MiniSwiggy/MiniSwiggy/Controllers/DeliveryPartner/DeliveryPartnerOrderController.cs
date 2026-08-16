using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Application.DeliveryPartner.DTOs;
using MiniSwiggy.Application.DeliveryPartner.Interfaces;
using MiniSwiggy.Infrastructure.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MiniSwiggy.API.Controllers.DeliveryPartner;

[ApiController]
[Authorize(Roles = "DeliveryPartner,DeliveryBoy")]
public class DeliveryPartnerOrderController : ControllerBase
{
    private readonly IDeliveryPartnerService _deliveryService;

    private readonly ApplicationDbContext _context;

    public DeliveryPartnerOrderController(IDeliveryPartnerService deliveryService, ApplicationDbContext context)
    {
        _deliveryService = deliveryService;
        _context = context;
    }

    private int GetDeliveryPartnerUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) 
                 ?? User.FindFirst("nameid") 
                 ?? User.FindFirst("sub") 
                 ?? User.FindFirst(ClaimTypes.Name);

        if (claim != null && int.TryParse(claim.Value, out int id))
        {
            return id;
        }

        var deliveryUser = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Role != null && (u.Role.Name == "DeliveryPartner" || u.Role.Name == "DeliveryBoy" || u.Role.Name.Contains("Delivery")));
        return deliveryUser?.Id ?? 1;
    }

    // GET /api/orders/my-assigned
    [HttpGet("api/orders/my-assigned")]
    public async Task<IActionResult> GetMyAssignedOrders()
    {
        var orders = await _deliveryService.GetMyAssignedOrdersAsync(GetDeliveryPartnerUserId());
        return Ok(orders);
    }

    // GET /api/orders/current
    [HttpGet("api/orders/current")]
    public async Task<IActionResult> GetCurrentDelivery()
    {
        var currentOrder = await _deliveryService.GetCurrentDeliveryAsync(GetDeliveryPartnerUserId());
        if (currentOrder == null) return NotFound(new { message = "No active delivery in progress." });
        return Ok(currentOrder);
    }

    // GET /api/delivery/history
    [HttpGet("api/delivery/history")]
    public async Task<IActionResult> GetDeliveryHistory()
    {
        var history = await _deliveryService.GetDeliveryHistoryAsync(GetDeliveryPartnerUserId());
        return Ok(history);
    }

    // GET /api/delivery/earnings
    [HttpGet("api/delivery/earnings")]
    public async Task<IActionResult> GetEarnings()
    {
        var earnings = await _deliveryService.GetEarningsAsync(GetDeliveryPartnerUserId());
        return Ok(earnings);
    }

    // POST /api/orders/accept
    [HttpPost("api/orders/accept")]
    public async Task<IActionResult> AcceptOrder([FromBody] StatusUpdateRequest request)
    {
        var result = await _deliveryService.AcceptOrderAsync(GetDeliveryPartnerUserId(), request.OrderId);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // POST /api/orders/reject
    [HttpPost("api/orders/reject")]
    public async Task<IActionResult> RejectOrder([FromBody] StatusUpdateRequest request)
    {
        var result = await _deliveryService.RejectOrderAsync(GetDeliveryPartnerUserId(), request.OrderId);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // POST /api/orders/reached-restaurant
    [HttpPost("api/orders/reached-restaurant")]
    public async Task<IActionResult> MarkReachedRestaurant([FromBody] StatusUpdateRequest request)
    {
        var result = await _deliveryService.MarkReachedRestaurantAsync(GetDeliveryPartnerUserId(), request.OrderId);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // POST /api/orders/picked-up
    [HttpPost("api/orders/picked-up")]
    public async Task<IActionResult> MarkPickedUp([FromBody] StatusUpdateRequest request)
    {
        var result = await _deliveryService.MarkPickedUpAsync(GetDeliveryPartnerUserId(), request.OrderId);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // POST /api/orders/out-for-delivery
    [HttpPost("api/orders/out-for-delivery")]
    public async Task<IActionResult> MarkOutForDelivery([FromBody] StatusUpdateRequest request)
    {
        var result = await _deliveryService.MarkOutForDeliveryAsync(GetDeliveryPartnerUserId(), request.OrderId);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // POST /api/orders/delivered
    [HttpPost("api/orders/delivered")]
    public async Task<IActionResult> MarkDelivered([FromBody] StatusUpdateRequest request)
    {
        var result = await _deliveryService.MarkDeliveredAsync(GetDeliveryPartnerUserId(), request.OrderId);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }
}
