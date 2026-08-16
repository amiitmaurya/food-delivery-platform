using MiniSwiggy.Application.DTOs.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniSwiggy.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync(string? searchQuery = null, string? roleFilter = null);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<(bool Success, string Message, int? UserId)> CreateUserAsync(CreateUserRequest request);
    Task<(bool Success, string Message)> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<(bool Success, string Message)> DeleteUserAsync(int id, int currentUserId);
    Task<(bool Success, string Message, bool NewStatus)> ToggleUserStatusAsync(int id, int currentUserId);
    Task<(bool Success, string Message)> AdminResetPasswordAsync(int id, AdminResetPasswordRequest request);
    Task<UserStatsDto> GetUserStatsAsync();
}
