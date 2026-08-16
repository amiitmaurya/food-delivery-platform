using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.Address;
using MiniSwiggy.Application.Interfaces;
using System.Security.Claims;

namespace MiniSwiggy.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
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

    // Add Address
    [HttpPost]
    public async Task<IActionResult> AddAddress(AddAddressRequest request)
    {
        var result = await _addressService.AddAddressAsync(GetUserId(), request);

        if (!result)
            return BadRequest(new { message = "Unable to add address." });

        return Ok(new { message = "Address added successfully." });
    }

    // Get My Addresses
    [HttpGet]
    public async Task<IActionResult> GetMyAddresses()
    {
        var result = await _addressService.GetMyAddressesAsync(GetUserId());

        return Ok(result ?? new List<AddressResponse>());
    }

    // Get Address By Id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAddressById(int id)
    {
        var result = await _addressService.GetAddressByIdAsync(id);

        if (result == null)
            return NotFound(new { message = "Address not found." });

        return Ok(result);
    }

    // Update Address
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(int id, UpdateAddressRequest request)
    {
        var result = await _addressService.UpdateAddressAsync(id, request);

        if (!result)
            return BadRequest(new { message = "Unable to update address." });

        return Ok(new { message = "Address updated successfully." });
    }

    // Delete Address
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var result = await _addressService.DeleteAddressAsync(id);

        if (!result)
            return NotFound(new { message = "Address not found." });

        return Ok(new { message = "Address deleted successfully." });
    }

    // Set Default Address
    [HttpPut("default/{id}")]
    public async Task<IActionResult> SetDefaultAddress(int id)
    {
        var result = await _addressService.SetDefaultAddressAsync(GetUserId(), id);

        if (!result)
            return BadRequest(new { message = "Unable to set default address." });

        return Ok(new { message = "Default address updated successfully." });
    }
}