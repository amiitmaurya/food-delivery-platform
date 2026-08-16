using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Configurations;

public class FileSettings
{
    public const string SectionName = "FileSettings";

    public string UploadPath { get; set; } = "wwwroot/uploads";

    public long MaxFileSize { get; set; } = 5 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];
}
