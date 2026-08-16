using MiniSwiggy.Application.DTOs.Address;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IAddressService
{
    Task<bool> AddAddressAsync(int userId, AddAddressRequest request);

    Task<bool> UpdateAddressAsync(int addressId, UpdateAddressRequest request);

    Task<bool> DeleteAddressAsync(int addressId);

    Task<IEnumerable<AddressResponse>> GetMyAddressesAsync(int userId);

    Task<AddressResponse?> GetAddressByIdAsync(int addressId);

    Task<bool> SetDefaultAddressAsync(int userId, int addressId);
}
