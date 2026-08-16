using MiniSwiggy.Domain.Entities;

namespace MiniSwiggy.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }

    IRoleRepository Roles { get; }

    IRestaurantRepository Restaurants { get; }

    ICategoryRepository Categories { get; }

    IFoodItemRepository FoodItems { get; }

    ICartRepository Carts { get; }

    ICartItemRepository CartItems { get; }

    IOrderRepository Orders { get; }

    IOrderItemRepository OrderItems { get; }

    IWishlistRepository Wishlists { get; }

    IWishlistItemRepository WishlistItems { get; }

    IReviewRepository Reviews { get; }

    IAddressRepository Addresses { get; }

    ICouponRepository Coupons { get; }

    IPaymentRepository Payments { get; }

    Task<int> SaveChangesAsync();
} 