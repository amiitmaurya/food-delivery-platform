using MiniSwiggy.Application.DTOs.Permission;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniSwiggy.Application.Interfaces;

public interface IModulePermissionService
{
    Task<IEnumerable<UserModulePermissionDto>> GetUserPermissionsAsync(int userId);
    Task<IEnumerable<UserModulePermissionDto>> GetMyPermissionsAsync(int currentUserId);
    Task<bool> UpdateUserPermissionsAsync(UpdateUserPermissionsRequest request);
    Task<bool> ResetUserPermissionsAsync(int userId);
}
