namespace MiniSwiggy.Application.DeliveryPartner.DTOs;

public class StatusUpdateRequest
{
    public int OrderId { get; set; }
    public string? Remarks { get; set; }
}

public class ToggleOnlineStatusRequest
{
    public bool IsOnline { get; set; }
}
