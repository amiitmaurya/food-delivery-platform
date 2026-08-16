using Microsoft.AspNetCore.Http;
using MiniSwiggy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IFileService
{
    Task<string> UploadImageAsync(
        IFormFile file,
        UploadFolder folder);

    Task DeleteImageAsync(string imageUrl);

}
