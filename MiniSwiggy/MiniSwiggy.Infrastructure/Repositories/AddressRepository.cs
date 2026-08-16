using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace MiniSwiggy.Infrastructure.Repositories;

public class AddressRepository : Repository<Address>, IAddressRepository
{
    private readonly ApplicationDbContext _context;

    public AddressRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Address>> GetByUserIdAsync(int userId)
    {
        return await _context.Addresses
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedOn)
            .ToListAsync();
    }

    public async Task<Address?> GetDefaultAddressAsync(int userId)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.IsDefault &&
                !x.IsDeleted);
    }
}
