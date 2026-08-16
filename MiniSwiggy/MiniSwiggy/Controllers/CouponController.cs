using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.Coupon;
using MiniSwiggy.Application.Interfaces;

namespace MiniSwiggy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    // GET: api/Coupon
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _couponService.GetAllAsync();
        return Ok(result);
    }

    // GET: api/Coupon/5
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _couponService.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    // POST: api/Coupon
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create(CreateCouponRequest request)
    {
        var result = await _couponService.CreateAsync(request);

        if (!result)
            return BadRequest(new { message = "Coupon already exists." });

        return Ok(new { message = "Coupon created successfully." });
    }

    // PUT: api/Coupon
    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(UpdateCouponRequest request)
    {
        var result = await _couponService.UpdateAsync(request);

        if (!result)
            return NotFound(new { message = "Coupon not found." });

        return Ok(new { message = "Coupon updated successfully." });
    }

    // DELETE: api/Coupon/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _couponService.DeleteAsync(id);

        if (!result)
            return NotFound(new { message = "Coupon not found." });

        return Ok(new { message = "Coupon deleted successfully." });
    }

    // POST: api/Coupon/apply
    [HttpPost("apply")]
    [Authorize]
    public async Task<IActionResult> ApplyCoupon(ApplyCouponRequest request)
    {
        var result = await _couponService.ApplyCouponAsync(request);

        if (!result.IsValid)
            return BadRequest(result);

        return Ok(result);
    }
}  