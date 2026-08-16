using MiniSwiggy.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentResponse>> GetAllAsync();

    Task<PaymentResponse?> GetByIdAsync(int id);

    Task<bool> CreateAsync(CreatePaymentRequest request);

    Task<bool> UpdatePaymentStatusAsync(UpdatePaymentStatusRequest request);

    Task<List<PaymentResponse>> GetMyPaymentsAsync(int userId);
}
