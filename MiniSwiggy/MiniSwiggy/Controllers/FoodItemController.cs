using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.Common;
using MiniSwiggy.Application.DTOs.FoodItem;
using MiniSwiggy.Application.Interfaces;

namespace MiniSwiggy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FoodItemController : ControllerBase
{
    private readonly IFoodItemService _foodItemService;

    public FoodItemController(IFoodItemService foodItemService)
    {
        _foodItemService = foodItemService;
    }

    // GET: api/FoodItem
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _foodItemService.GetAllAsync();
        return Ok(result);
    }

    // GET: api/FoodItem/category/1
    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var result = await _foodItemService.GetByCategoryAsync(categoryId);
        return Ok(result);
    }

    // GET: api/FoodItem/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _foodItemService.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    // POST: api/FoodItem
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create(CreateFoodItemRequest request)
    {
        var id = await _foodItemService.CreateAsync(request);

        if (id <= 0)
            return BadRequest(new { message = "Unable to create food item." });

        return Ok(new { id = id, message = "Food item created successfully." });
    }

    // PUT: api/FoodItem
    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(UpdateFoodItemRequest request)
    {
        var result = await _foodItemService.UpdateAsync(request);

        if (!result)
            return NotFound(new { message = "Food item not found." });

        return Ok(new { message = "Food item updated successfully." });
    }

    // DELETE: api/FoodItem/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _foodItemService.DeleteAsync(id);

        if (!result)
            return NotFound(new { message = "Food item not found." });

        return Ok(new { message = "Food item deleted successfully." });
    }

    // GET: api/FoodItem/search
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] FoodFilterRequest request)
    {
        var result = await _foodItemService.SearchFoodsAsync(request);

        return Ok(result);
    }

    [HttpPost("{id}/upload-image")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UploadImage(
    int id,
    [FromForm] UploadFoodItemImageRequest request)
    {
        var imageUrl = await _foodItemService.UploadImageAsync(
            id,
            request.File);

        return Ok(new
        {
            Message = "Food item image uploaded successfully.",
            ImageUrl = imageUrl
        });
    }

}             