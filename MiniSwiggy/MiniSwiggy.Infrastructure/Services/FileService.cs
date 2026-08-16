using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MiniSwiggy.Application.Configurations;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly FileSettings _settings;

    public FileService(IOptions<FileSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<string> UploadImageAsync(
    IFormFile file,
    UploadFolder folder)
    {
        if (file == null || file.Length == 0)
            throw new Exception("No file selected.");

        if (file.Length > _settings.MaxFileSize)
            throw new Exception("File size exceeds limit.");

        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!_settings.AllowedExtensions.Contains(extension))
            throw new Exception("Invalid file format.");

        var folderName = folder.ToString().ToLower();

        var folderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            _settings.UploadPath,
            folderName);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid()}{extension}";

        var filePath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);

        await file.CopyToAsync(stream);

        return $"/uploads/{folderName}/{fileName}";
    }

    public Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return Task.CompletedTask;

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            imageUrl.TrimStart('/').Replace("/", "\\"));

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
