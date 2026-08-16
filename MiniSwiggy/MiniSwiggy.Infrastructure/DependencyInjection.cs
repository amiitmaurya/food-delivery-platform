using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Infrastructure.Data;
using MiniSwiggy.Infrastructure.Repositories;
using MiniSwiggy.Infrastructure.Services;


namespace MiniSwiggy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));


        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();



        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryService, CategoryService>();


        services.AddScoped<IFoodItemRepository, FoodItemRepository>();

        services.AddScoped<IFoodItemService, FoodItemService>();


        services.AddScoped<ICartRepository, CartRepository>();

        services.AddScoped<ICartItemRepository, CartItemRepository>();

        services.AddScoped<ICartService, CartService>();

        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IOrderItemRepository, OrderItemRepository>();

        services.AddScoped<IOrderService, OrderService>();

        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<IWishlistItemRepository, WishlistItemRepository>();
        services.AddScoped<IWishlistService, WishlistService>();

        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IReviewService, ReviewService>();


        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IAddressService, AddressService>();


        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<ICouponRepository, CouponRepository>();

        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<IFileService, FileService>();

        services.AddScoped<MiniSwiggy.Application.DeliveryPartner.Interfaces.IDeliveryPartnerService, MiniSwiggy.Infrastructure.Services.DeliveryPartner.DeliveryPartnerService>();

        services.AddScoped<IModulePermissionService, ModulePermissionService>();

        return services;
    }
}