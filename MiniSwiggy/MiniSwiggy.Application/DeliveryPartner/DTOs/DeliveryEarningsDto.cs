using System;
using System.Collections.Generic;

namespace MiniSwiggy.Application.DeliveryPartner.DTOs;

public class DeliveryEarningsDto
{
    public decimal TodayEarnings { get; set; }
    public decimal WeeklyEarnings { get; set; }
    public decimal MonthlyEarnings { get; set; }
    public decimal TotalEarnings { get; set; }
    public int TodayDeliveriesCount { get; set; }
    public int TotalDeliveriesCount { get; set; }
    public double AverageRating { get; set; }

    public List<DailyEarningBreakdownDto> DailyBreakdown { get; set; } = new();
    public List<RecentPayoutDto> RecentPayouts { get; set; } = new();
}

public class DailyEarningBreakdownDto
{
    public string Date { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int DeliveriesCount { get; set; }
}

public class RecentPayoutDto
{
    public int Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
}
