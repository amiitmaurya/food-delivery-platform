using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DeliveryPartner.DTOs;
using MiniSwiggy.Application.DeliveryPartner.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MiniSwiggy.API.Controllers.DeliveryPartner;

[ApiController]
[Authorize(Roles = "SuperAdmin,Admin,DeliveryPartner,DeliveryBoy")]
public class DeliveryPartnerProfileController : ControllerBase
{
    private readonly IDeliveryPartnerService _deliveryService;

    public DeliveryPartnerProfileController(IDeliveryPartnerService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    private int GetDeliveryPartnerUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 1;
    }

    // GET /api/profile
    [HttpGet("api/profile")]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _deliveryService.GetProfileAsync(GetDeliveryPartnerUserId());
        return Ok(profile);
    }

    // PUT /api/profile/delivery-details
    [HttpPut("api/profile/delivery-details")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateDeliveryProfileRequest request)
    {
        var success = await _deliveryService.UpdateProfileAsync(GetDeliveryPartnerUserId(), request);
        if (!success) return BadRequest(new { message = "Failed to update profile." });
        return Ok(new { message = "Delivery profile updated successfully." });
    }

    // POST /api/profile/change-password
    [HttpPost("api/profile/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest(new { message = "New password and Confirm password do not match." });

        var success = await _deliveryService.ChangePasswordAsync(GetDeliveryPartnerUserId(), request);
        if (!success) return BadRequest(new { message = "Failed to change password." });
        return Ok(new { message = "Password updated successfully." });
    }

    // POST /api/profile/toggle-online
    [HttpPost("api/profile/toggle-online")]
    public async Task<IActionResult> ToggleOnlineStatus([FromBody] ToggleOnlineStatusRequest request)
    {
        var success = await _deliveryService.ToggleOnlineStatusAsync(GetDeliveryPartnerUserId(), request.IsOnline);
        if (!success) return BadRequest(new { message = "Failed to toggle status." });
        return Ok(new { message = "Online status updated.", isOnline = request.IsOnline });
    }

    // GET /api/DeliveryPartner/all-partners
    [HttpGet("api/DeliveryPartner/all-partners")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAllPartners()
    {
        var partners = await _deliveryService.GetAllPartnersAsync();
        return Ok(partners);
    }
}
