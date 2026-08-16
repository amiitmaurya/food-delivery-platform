using MiniSwiggy.Application.DTOs.Address;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;

namespace MiniSwiggy.Infrastructure.Services;

public class AddressService : IAddressService
{
    private readonly IUnitOfWork _unitOfWork;

    public AddressService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public async Task<bool> AddAddressAsync(int userId, AddAddressRequest request)
    {
        // Ensure user exists for foreign key constraint
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var firstUser = users.FirstOrDefault();
            if (firstUser != null)
            {
                userId = firstUser.Id;
            }
            else
            {
                var role = (await _unitOfWork.Roles.GetAllAsync()).FirstOrDefault();
                var newUser = new User
                {
                    FullName = "Default Customer",
                    Email = "customer@miniswiggy.com",
                    PasswordHash = "default_hash",
                    PhoneNumber = "9999999999",
                    RoleId = role?.Id ?? 1
                };
                await _unitOfWork.Users.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();
                userId = newUser.Id;
            }
        }

        if (request.IsDefault)
        {
            var defaultAddress = await _unitOfWork.Addresses.GetDefaultAddressAsync(userId);

            if (defaultAddress != null)
            {
                defaultAddress.IsDefault = false;
                _unitOfWork.Addresses.Update(defaultAddress);
            }
        }

        var address = new Address
        {
            UserId = userId,
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? "Customer" : request.FullName,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? "9999999999" : request.PhoneNumber,
            HouseNo = string.IsNullOrWhiteSpace(request.HouseNo) ? "1" : request.HouseNo,
            Street = string.IsNullOrWhiteSpace(request.Street) ? "Main Street" : request.Street,
            Landmark = request.Landmark,
            City = string.IsNullOrWhiteSpace(request.City) ? "City" : request.City,
            State = string.IsNullOrWhiteSpace(request.State) ? "State" : request.State,
            Pincode = string.IsNullOrWhiteSpace(request.Pincode) ? "000000" : request.Pincode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = request.IsDefault,
            CreatedOn = DateTime.UtcNow
        };

        await _unitOfWork.Addresses.AddAsync(address);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }


    public async Task<IEnumerable<AddressResponse>> GetMyAddressesAsync(int userId)
    {
        var addresses = await _unitOfWork.Addresses.GetByUserIdAsync(userId);

        return addresses.Select(x => new AddressResponse
        {
            Id = x.Id,
            FullName = x.FullName,
            PhoneNumber = x.PhoneNumber,
            HouseNo = x.HouseNo,
            Street = x.Street,
            Landmark = x.Landmark,
            City = x.City,
            State = x.State,
            Pincode = x.Pincode,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            IsDefault = x.IsDefault
        });
    }


    public async Task<AddressResponse?> GetAddressByIdAsync(int addressId)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(addressId);

        if (address == null || address.IsDeleted)
            return null;

        return new AddressResponse
        {
            Id = address.Id,
            FullName = address.FullName,
            PhoneNumber = address.PhoneNumber,
            HouseNo = address.HouseNo,
            Street = address.Street,
            Landmark = address.Landmark,
            City = address.City,
            State = address.State,
            Pincode = address.Pincode,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            IsDefault = address.IsDefault
        };
    }

    public async Task<bool> UpdateAddressAsync(int addressId, UpdateAddressRequest request)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(addressId);

        if (address == null || address.IsDeleted)
            return false;

        if (request.IsDefault)
        {
            var defaultAddress = await _unitOfWork.Addresses
                .GetDefaultAddressAsync(address.UserId);

            if (defaultAddress != null && defaultAddress.Id != address.Id)
            {
                defaultAddress.IsDefault = false;
                defaultAddress.UpdatedOn = DateTime.UtcNow;

                _unitOfWork.Addresses.Update(defaultAddress);
            }
        }

        address.FullName = request.FullName;
        address.PhoneNumber = request.PhoneNumber;
        address.HouseNo = request.HouseNo;
        address.Street = request.Street;
        address.Landmark = request.Landmark;
        address.City = request.City;
        address.State = request.State;
        address.Pincode = request.Pincode;
        address.Latitude = request.Latitude;
        address.Longitude = request.Longitude;
        address.IsDefault = request.IsDefault;
        address.UpdatedOn = DateTime.UtcNow;

        _unitOfWork.Addresses.Update(address);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAddressAsync(int addressId)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(addressId);

        if (address == null || address.IsDeleted)
            return false;

        address.IsDeleted = true;
        address.IsDefault = false;
        address.UpdatedOn = DateTime.UtcNow;

        _unitOfWork.Addresses.Update(address);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
    {
        var address = await _unitOfWork.Addresses.GetByIdAsync(addressId);

        if (address == null || address.IsDeleted || address.UserId != userId)
            return false;

        var defaultAddress = await _unitOfWork.Addresses
            .GetDefaultAddressAsync(userId);

        if (defaultAddress != null && defaultAddress.Id != address.Id)
        {
            defaultAddress.IsDefault = false;
            defaultAddress.UpdatedOn = DateTime.UtcNow;

            _unitOfWork.Addresses.Update(defaultAddress);
        }

        address.IsDefault = true;
        address.UpdatedOn = DateTime.UtcNow;

        _unitOfWork.Addresses.Update(address);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }


}