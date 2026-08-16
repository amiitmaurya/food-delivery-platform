using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.Restaurant;
using MiniSwiggy.Application.Interfaces;

namespace MiniSwiggy.Controllers;
[Route("api/[controller]")]
[ApiController]
public class RestaurantController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;

    public RestaurantController(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {

        var result = await _restaurantService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _restaurantService.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateRestaurantRequest request)
    {
        var id = await _restaurantService.CreateAsync(request);

        if (id <= 0)
            return BadRequest(new { message = "Unable to create restaurant." });

        return Ok(new
        {
            Id = id,
            Message = "Restaurant created successfully."
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(
    int id,
    [FromBody] UpdateRestaurantRequest request)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _restaurantService.UpdateAsync(request);

        if (!result)
            return NotFound();

        return Ok(new
        {
            Message = "Restaurant updated successfully."
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _restaurantService.DeleteAsync(id);

        if (!result)
            return NotFound(new
            {
                Message = "Restaurant not found."
            });

        return Ok(new
        {
            Message = "Restaurant deleted successfully."
        });
    }

    [HttpPost("{id}/upload-image")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UploadImage(
    int id,
    [FromForm] UploadRestaurantImageRequest request)
    {
        var imageUrl = await _restaurantService.UploadImageAsync(id, request.Image);

        return Ok(new
        {
            Message = "Image uploaded successfully.",
            ImageUrl = imageUrl
        });
    }

}

