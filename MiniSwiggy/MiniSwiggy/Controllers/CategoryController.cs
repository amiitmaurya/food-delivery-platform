using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.Category;
using MiniSwiggy.Application.Interfaces;

namespace MiniSwiggy.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }


    [HttpGet("restaurant/{restaurantId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByRestaurant(int restaurantId)
    {
        var result = await _categoryService.GetAllByRestaurantAsync(restaurantId);
        return Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var id = await _categoryService.CreateAsync(request);

        if (id <= 0)
            return BadRequest(new { message = "Category already exists for this restaurant." });

        return Ok(new { id = id, message = "Category created successfully." });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(int id, UpdateCategoryRequest request)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _categoryService.UpdateAsync(request);

        if (!result)
            return NotFound();

        return Ok(new { message = "Category updated successfully." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);

        if (!result)
            return NotFound();

        return Ok(new { message = "Category deleted successfully." });
    }

    [HttpPost("{id}/upload-image")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UploadImage(
    int id,
    [FromForm] UploadCategoryImageRequest request)
    {
        var imageUrl = await _categoryService.UploadImageAsync(
            id,
            request.Image);

        return Ok(new
        {
            Message = "Category image uploaded successfully.",
            ImageUrl = imageUrl
        });
    }

    
}
 