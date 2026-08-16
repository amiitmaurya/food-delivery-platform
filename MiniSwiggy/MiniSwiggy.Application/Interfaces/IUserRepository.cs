using Microsoft.AspNetCore.Http;
using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);

    Task<bool> IsEmailExistsAsync(string email);

    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<bool> IsPhoneExistsAsync(string phoneNumber);

    
}
