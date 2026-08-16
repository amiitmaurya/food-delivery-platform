using MiniSwiggy.Domain.Common;

using MiniSwiggy.Domain.Enums;


namespace MiniSwiggy.Domain.Entities;

public class Order : BaseEntity
{
    public int UserId { get; set; }

    public int RestaurantId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public decimal DeliveryCharge { get; set; }

    public decimal Discount { get; set; }

    public decimal Tax { get; set; }

    public decimal FinalAmount { get; set; }

    public string DeliveryAddress { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? DeliveredDate { get; set; }

    // Navigation Properties

    public User User { get; set; } = null!;

    public Restaurant Restaurant { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
