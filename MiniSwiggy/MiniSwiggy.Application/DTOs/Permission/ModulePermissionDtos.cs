using System.Collections.Generic;

namespace MiniSwiggy.Application.DTOs.Permission;

public class UserModulePermissionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleCategory { get; set; } = string.Empty;
    public string RoutePath { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
}

public class UpdateUserPermissionsRequest
{
    public int UserId { get; set; }
    public List<ModuleToggleDto> Permissions { get; set; } = new();
}

public class ModuleToggleDto
{
    public string ModuleKey { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
}
