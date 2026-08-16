using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Restaurant;

public class UploadRestaurantImageRequest
{
    public IFormFile Image { get; set; } = default!;
}
