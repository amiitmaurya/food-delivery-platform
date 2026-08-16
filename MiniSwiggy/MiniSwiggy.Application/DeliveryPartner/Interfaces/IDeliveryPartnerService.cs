using System.Collections.Generic;
using System.Threading.Tasks;
using MiniSwiggy.Application.DeliveryPartner.DTOs;

namespace MiniSwiggy.Application.DeliveryPartner.Interfaces;

public interface IDeliveryPartnerService
{
    Task<List<DeliveryOrderDto>> GetMyAssignedOrdersAsync(int deliveryPartnerUserId);
    Task<DeliveryOrderDto?> GetCurrentDeliveryAsync(int deliveryPartnerUserId);
    Task<List<DeliveryOrderDto>> GetDeliveryHistoryAsync(int deliveryPartnerUserId);
    Task<DeliveryEarningsDto> GetEarningsAsync(int deliveryPartnerUserId);
    Task<DeliveryProfileDto> GetProfileAsync(int deliveryPartnerUserId);
    Task<List<DeliveryProfileDto>> GetAllPartnersAsync();
    Task<bool> UpdateProfileAsync(int deliveryPartnerUserId, UpdateDeliveryProfileRequest request);
    Task<bool> ChangePasswordAsync(int deliveryPartnerUserId, ChangePasswordRequest request);
    Task<bool> ToggleOnlineStatusAsync(int deliveryPartnerUserId, bool isOnline);

    Task<(bool Success, string Message)> AcceptOrderAsync(int deliveryPartnerUserId, int orderId);
    Task<(bool Success, string Message)> RejectOrderAsync(int deliveryPartnerUserId, int orderId);
    Task<(bool Success, string Message)> MarkReachedRestaurantAsync(int deliveryPartnerUserId, int orderId);
    Task<(bool Success, string Message)> MarkPickedUpAsync(int deliveryPartnerUserId, int orderId);
    Task<(bool Success, string Message)> MarkOutForDeliveryAsync(int deliveryPartnerUserId, int orderId);
    Task<(bool Success, string Message)> MarkDeliveredAsync(int deliveryPartnerUserId, int orderId);
}
