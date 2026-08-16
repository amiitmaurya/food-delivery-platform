using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Auth;

public class UploadUserProfileImageRequest
{
    public IFormFile File { get; set; } = default!;
}
 

