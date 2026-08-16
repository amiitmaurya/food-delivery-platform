using MiniSwiggy.Domain.Entities;

namespace MiniSwiggy.Application.Interfaces;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(int userId);
}