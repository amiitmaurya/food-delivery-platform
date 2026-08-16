
using MiniSwiggy.Application.Interfaces;

using MiniSwiggy.Infrastructure.Data;

namespace MiniSwiggy.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IUserRepository Users { get; }

    public IRoleRepository Roles { get; }

    public IRestaurantRepository Restaurants { get; }

    public ICategoryRepository Categories { get; }

    public IFoodItemRepository FoodItems { get; }

    public ICartRepository Carts { get; }

    public ICartItemRepository CartItems { get; }

    public IOrderRepository Orders { get; }

    public IOrderItemRepository OrderItems { get; }


    public IWishlistRepository Wishlists { get; }

    public IWishlistItemRepository WishlistItems { get; }

    public IReviewRepository Reviews { get; }

    public IAddressRepository Addresses { get; }

    public ICouponRepository Coupons { get; }
    public IPaymentRepository Payments { get; }



    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        Users = new UserRepository(context);
        Roles = new RoleRepository(context);
        Restaurants = new RestaurantRepository(context);
        Categories = new CategoryRepository(context);
        FoodItems = new FoodItemRepository(context);
        Carts = new CartRepository(context);

        CartItems = new CartItemRepository(context);

        Orders = new OrderRepository(context);

        OrderItems = new OrderItemRepository(context);

        Wishlists = new WishlistRepository(context);

        WishlistItems = new WishlistItemRepository(context);


        Reviews = new ReviewRepository(context);

        Addresses = new AddressRepository(context);

        Coupons = new CouponRepository(context);

        Payments = new PaymentRepository(context);

    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}