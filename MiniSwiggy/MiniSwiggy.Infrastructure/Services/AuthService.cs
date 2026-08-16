using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MiniSwiggy.Application.DTOs.Auth;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Domain.Enums;
using MiniSwiggy.Shared.Exceptions;

namespace MiniSwiggy.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IFileService _fileService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IFileService fileService,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _unitOfWork.Users.IsEmailExistsAsync(request.Email))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Email already exists."
            };
        }

        // 2. Phone Check
        var existingUser = await _unitOfWork.Users.GetByPhoneNumberAsync(request.PhoneNumber);

        if (existingUser != null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Phone number already exists."
            };
        }

        var customerRole = await _unitOfWork.Roles.GetByNameAsync("Customer");

        if (customerRole == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Customer role not found."
            };
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            RoleId = customerRole.Id,
            IsActive = true,
            ImageUrl = request.ImageUrl
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "User registered successfully."
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(
            request.Password,
            user.PasswordHash);

        if (!isPasswordValid)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        var token = _jwtTokenService.GenerateToken(user);

        user.LastLogin = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful.",
            Token = token,
            FullName = user.FullName,
            ImageUrl = user.ImageUrl,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role?.Name
        };
    }

    public async Task<string> UploadProfileImageAsync(
    int userId,
    IFormFile file)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user is null)
            throw new NotFoundException("User not found.");

        var oldImage = user.ImageUrl;

        var imageUrl = await _fileService.UploadImageAsync(
            file,
            UploadFolder.Users);

        try
        {
            user.ImageUrl = imageUrl;

            _unitOfWork.Users.Update(user);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "User profile image uploaded successfully. UserId: {UserId}",
                userId);

            if (!string.IsNullOrWhiteSpace(oldImage))
            {
                try
                {
                    await _fileService.DeleteImageAsync(oldImage);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to delete old profile image.");
                }
            }

            return imageUrl;
        }
        catch
        {
            try
            {
                await _fileService.DeleteImageAsync(imageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to rollback uploaded profile image.");
            }

            throw;
        }
    }

    public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
            return false;

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedOn = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
            return false;

        if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return false;

        var isOldValid = _passwordHasher.VerifyPassword(request.OldPassword, user.PasswordHash);
        if (!isOldValid)
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedOn = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
            return null;

        var namePart = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName.Split(' ')[0] : "User";

        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role?.Name,
            ImageUrl = user.ImageUrl,
            CurrentPasswordHint = $"{namePart}@MiniSwiggy{user.Id}"
        };
    }
}
