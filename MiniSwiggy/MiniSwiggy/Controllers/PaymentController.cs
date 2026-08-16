using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.Payment;
using MiniSwiggy.Application.Interfaces;
using System.Security.Claims;

namespace MiniSwiggy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
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

    private bool IsAdminOrSuperAdmin()
    {
        return User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
    }

    // GET: api/Payment (Admin only)
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _paymentService.GetAllAsync();
        return Ok(result);
    }

    // GET: api/Payment/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _paymentService.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    // POST: api/Payment
    [HttpPost]
    public async Task<IActionResult> Create(CreatePaymentRequest request)
    {
        var result = await _paymentService.CreateAsync(request);

        if (!result)
            return BadRequest(new { message = "Unable to create payment." });

        return Ok(new { message = "Payment created successfully." });
    }

    // PUT: api/Payment/status
    [HttpPut("status")]
    public async Task<IActionResult> UpdateStatus(UpdatePaymentStatusRequest request)
    {
        var result = await _paymentService.UpdatePaymentStatusAsync(request);

        if (!result)
            return NotFound(new { message = "Payment not found." });

        return Ok(new { message = "Payment updated successfully." });
    }

    // GET: api/Payment/my-payments
    [HttpGet("my-payments")]
    public async Task<IActionResult> GetMyPayments()
    {
        var result = await _paymentService.GetMyPaymentsAsync(GetUserId());
        return Ok(result);
    }

    // GET: api/Payment/user/5 (Admin or Self only)
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPayments(int userId)
    {
        if (userId != GetUserId() && !IsAdminOrSuperAdmin())
        {
            return Forbid();
        }

        var result = await _paymentService.GetMyPaymentsAsync(userId);
        return Ok(result);
    }
}