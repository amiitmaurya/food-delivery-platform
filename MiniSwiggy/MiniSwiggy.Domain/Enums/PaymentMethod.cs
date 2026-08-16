using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Enums;

public enum PaymentMethod
{
    CashOnDelivery = 1,
    UPI = 2,
    Card = 3,
    NetBanking = 4,
    Wallet = 5,
    Razorpay = 6,
    Stripe = 7
}