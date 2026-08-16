using MiniSwiggy.Domain.Common;
using System;

namespace MiniSwiggy.Domain.Entities.DeliveryPartner;

public class DeliveryOrderTracker : BaseEntity
{
    public int OrderId { get; set; }
    public int DeliveryPartnerUserId { get; set; }
    public string DeliveryStatus { get; set; } = "Assigned"; // Assigned, Accepted, ReachedRestaurant, PickedUp, OutForDelivery, Delivered, Rejected

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ReachedRestaurantAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? OutForDeliveryAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public double? CustomerRating { get; set; }
    public string? CustomerFeedback { get; set; }
    public decimal DeliveryEarnings { get; set; } = 40.00m; // Default delivery payout per order
}
