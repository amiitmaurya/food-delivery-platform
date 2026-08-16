using MiniSwiggy.Application.DTOs.Role;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniSwiggy.Application.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(int id);
    Task<(bool Success, string Message, int? RoleId)> CreateRoleAsync(CreateRoleRequest request);
    Task<(bool Success, string Message)> UpdateRoleAsync(int id, UpdateRoleRequest request);
    Task<(bool Success, string Message)> DeleteRoleAsync(int id);
}
