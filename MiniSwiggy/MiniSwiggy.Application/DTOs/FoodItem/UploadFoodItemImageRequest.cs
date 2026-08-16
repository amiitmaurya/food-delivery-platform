using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.FoodItem;

public class UploadFoodItemImageRequest
{
    public IFormFile File { get; set; } = default!;
}
