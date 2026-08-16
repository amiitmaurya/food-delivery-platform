using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.Review;
using MiniSwiggy.Application.Interfaces;
using System.Security.Claims;

namespace MiniSwiggy.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
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

    // Add Review
    [HttpPost]
    public async Task<IActionResult> AddReview(AddReviewRequest request)
    {
        var result = await _reviewService.AddReviewAsync(GetUserId(), request);

        if (!result)
            return BadRequest(new { message = "Review already exists, invalid rating, or food item not found." });

        return Ok(new { message = "Review added successfully." });
    }

    // Get Reviews of Food Item
    [AllowAnonymous]
    [HttpGet("food/{foodItemId}")]
    public async Task<IActionResult> GetFoodReviews(int foodItemId)
    {
        var result = await _reviewService.GetFoodReviewsAsync(foodItemId);

        if (result == null)
            return Ok(new List<ReviewResponse>());

        return Ok(result);
    }

    // Get My Reviews
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReviews()
    {
        var result = await _reviewService.GetMyReviewsAsync(GetUserId());

        return Ok(result ?? new List<MyReviewResponse>());
    }

    // Update Review
    [HttpPut("{reviewId}")]
    public async Task<IActionResult> UpdateReview(int reviewId, UpdateReviewRequest request)
    {
        var result = await _reviewService.UpdateReviewAsync(reviewId, request);

        if (!result)
            return BadRequest(new { message = "Review not found or invalid rating." });

        return Ok(new { message = "Review updated successfully." });
    }

    // Delete Review
    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var result = await _reviewService.DeleteReviewAsync(reviewId);

        if (!result)
            return NotFound(new { message = "Review not found." });

        return Ok(new { message = "Review deleted successfully." });
    }

    [AllowAnonymous]
    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetRestaurantReviews(int restaurantId)
    {
        var result = await _reviewService.GetRestaurantReviewsAsync(restaurantId);

        if (result == null)
            return Ok(new List<ReviewResponse>());

        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAllReviews()
    {
        var result = await _reviewService.GetAllReviewsAsync();
        return Ok(result);
    }

}