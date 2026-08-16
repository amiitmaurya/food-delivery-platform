using MiniSwiggy.Domain.Common;
using System;

namespace MiniSwiggy.Domain.Entities.DeliveryPartner;

public class DeliveryPartnerProfile : BaseEntity
{
    public int UserId { get; set; }
    public bool IsOnline { get; set; } = true;

    // Vehicle Details
    public string VehicleType { get; set; } = "Bike";
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;

    // License Details
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseExpiryDate { get; set; } = string.Empty;

    // Bank Details
    public string BankAccountHolder { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string IfscCode { get; set; } = string.Empty;
    public string UpiId { get; set; } = string.Empty;

    // Rating aggregate
    public double Rating { get; set; } = 4.8;
    public int TotalRatingsCount { get; set; } = 12;

    public User User { get; set; } = null!;
}
