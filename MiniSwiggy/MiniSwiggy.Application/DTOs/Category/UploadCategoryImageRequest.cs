using Microsoft.AspNetCore.Http;

namespace MiniSwiggy.Application.DTOs.Category;

public class UploadCategoryImageRequest
{
    public IFormFile Image { get; set; } = null!;
}
