using MiniSwiggy.Application.DTOs.Payment;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Domain.Enums;

namespace MiniSwiggy.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PaymentResponse>> GetAllAsync()
    {
        await SyncMissingOrderPaymentsAsync();

        var payments = await _unitOfWork.Payments.GetAllWithDetailsAsync();

        return payments
            .Where(x => !x.IsDeleted)
            .Select(x => new PaymentResponse
            {
                Id = x.Id,
                OrderId = x.OrderId,
                OrderNumber = x.Order?.OrderNumber ?? $"ORD-{x.OrderId}",
                UserId = x.Order?.UserId ?? 0,
                CustomerName = x.Order?.User?.FullName ?? "Customer",
                CustomerEmail = x.Order?.User?.Email ?? string.Empty,
                Amount = x.Amount,
                PaymentMethod = x.PaymentMethod,
                PaymentStatus = x.PaymentStatus,
                TransactionId = x.TransactionId,
                GatewayOrderId = x.GatewayOrderId,
                CreatedOn = x.CreatedOn,
                PaidOn = x.PaidOn
            })
            .ToList();
    }

    private async Task SyncMissingOrderPaymentsAsync()
    {
        try
        {
            var allOrders = await _unitOfWork.Orders.GetAllAsync();
            var allPayments = await _unitOfWork.Payments.GetAllAsync();
            var existingOrderIds = allPayments.Select(p => p.OrderId).ToHashSet();

            bool hasNew = false;
            foreach (var order in allOrders.Where(o => !o.IsDeleted))
            {
                if (!existingOrderIds.Contains(order.Id))
                {
                    var newPayment = new Payment
                    {
                        OrderId = order.Id,
                        Amount = order.FinalAmount > 0 ? order.FinalAmount : order.TotalAmount,
                        PaymentMethod = order.PaymentMethod,
                        PaymentStatus = order.PaymentStatus,
                        CreatedOn = order.OrderDate != default ? order.OrderDate : DateTime.UtcNow,
                        PaidOn = order.PaymentStatus == PaymentStatus.Paid ? (order.UpdatedOn ?? DateTime.UtcNow) : null
                    };
                    await _unitOfWork.Payments.AddAsync(newPayment);
                    existingOrderIds.Add(order.Id);
                    hasNew = true;
                }
            }

            if (hasNew)
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch
        {
            // Silently proceed if sync fails
        }
    }


    public async Task<PaymentResponse?> GetByIdAsync(int id)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(id);

        if (payment == null || payment.IsDeleted)
            return null;

        return new PaymentResponse
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            OrderNumber = payment.Order?.OrderNumber ?? $"ORD-{payment.OrderId}",
            UserId = payment.Order?.UserId ?? 0,
            CustomerName = payment.Order?.User?.FullName ?? "Customer",
            CustomerEmail = payment.Order?.User?.Email ?? string.Empty,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
            TransactionId = payment.TransactionId,
            GatewayOrderId = payment.GatewayOrderId,
            CreatedOn = payment.CreatedOn,
            PaidOn = payment.PaidOn
        };
    }


    public async Task<bool> CreateAsync(CreatePaymentRequest request)
    {
        // Order exists?
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);

        if (order == null || order.IsDeleted)
            return false;

        // Duplicate payment check
        var existingPayment = await _unitOfWork.Payments
            .GetByOrderIdAsync(request.OrderId);

        if (existingPayment != null)
            return false;

        var payment = new Payment
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            CreatedOn = DateTime.UtcNow
        };

        await _unitOfWork.Payments.AddAsync(payment);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdatePaymentStatusAsync(UpdatePaymentStatusRequest request)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(request.PaymentId);

        if (payment == null || payment.IsDeleted)
            return false;

        payment.PaymentStatus = request.PaymentStatus;
        payment.TransactionId = request.TransactionId;
        payment.GatewayOrderId = request.GatewayOrderId;
        payment.UpdatedOn = DateTime.UtcNow;

        if (request.PaymentStatus == PaymentStatus.Paid)
        {
            payment.PaidOn = DateTime.UtcNow;
        }

        _unitOfWork.Payments.Update(payment);

        // Sync with Orders table
        var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
        if (order != null)
        {
            order.PaymentStatus = request.PaymentStatus;
            order.UpdatedOn = DateTime.UtcNow;
            _unitOfWork.Orders.Update(order);
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }


    public async Task<List<PaymentResponse>> GetMyPaymentsAsync(int userId)
    {
        await SyncMissingOrderPaymentsAsync();

        var payments = await _unitOfWork.Payments
            .GetByUserPaymentsAsync(userId);

        return payments.Select(x => new PaymentResponse
        {
            Id = x.Id,
            OrderId = x.OrderId,
            OrderNumber = x.Order?.OrderNumber ?? $"ORD-{x.OrderId}",
            UserId = x.Order?.UserId ?? userId,
            CustomerName = x.Order?.User?.FullName ?? "Customer",
            CustomerEmail = x.Order?.User?.Email ?? string.Empty,
            Amount = x.Amount,
            PaymentMethod = x.PaymentMethod,
            PaymentStatus = x.PaymentStatus,
            TransactionId = x.TransactionId,
            GatewayOrderId = x.GatewayOrderId,
            CreatedOn = x.CreatedOn,
            PaidOn = x.PaidOn
        }).ToList();
    }  



} 