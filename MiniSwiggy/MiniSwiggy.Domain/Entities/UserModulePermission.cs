using MiniSwiggy.Domain.Common;
using System;

namespace MiniSwiggy.Domain.Entities;

public class UserModulePermission : BaseEntity
{
    public int UserId { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleCategory { get; set; } = string.Empty;
    public string RoutePath { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public bool IsAllowed { get; set; } = true;

    // Navigation Property
    public User? User { get; set; }
}
