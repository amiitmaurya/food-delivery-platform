namespace MiniSwiggy.Application.DTOs.Role;

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UsersCount { get; set; }
}
