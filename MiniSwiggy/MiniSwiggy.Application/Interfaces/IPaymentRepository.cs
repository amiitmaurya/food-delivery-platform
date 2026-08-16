using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(int orderId);

    Task<List<Payment>> GetByUserPaymentsAsync(int userId);

    Task<List<Payment>> GetAllWithDetailsAsync();
}
