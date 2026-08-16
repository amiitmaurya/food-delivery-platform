namespace MiniSwiggy.Application.DTOs.User;

public class UserStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalSuperAdmins { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalDeliveryPartners { get; set; }
    public int TotalRestaurantOwners { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
}
