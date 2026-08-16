using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;

namespace MiniSwiggy.Infrastructure.Repositories;

public class UserRepository
    : Repository<User>, IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(x => x.Email == email);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
    }

    public async Task<bool> IsPhoneExistsAsync(string phoneNumber)
    {
        return await _context.Users
            .AnyAsync(x => x.PhoneNumber == phoneNumber);
    }
} 