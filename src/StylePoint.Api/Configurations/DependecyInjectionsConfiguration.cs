using AutoLedger.Infrastructure.Persistence.Repositories;
using CloudinaryDotNet;
using FluentValidation;
using StylePoint.Application.Dtos;
using StylePoint.Application.Helpers;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Implementations;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Application.Validators;
using StylePoint.Infrastructure.Persistence.Repositories;

namespace AutoLedger.Api.Configurations;

public static class DependecyInjectionsConfiguration
{
    public static void ConfigureDependecies(this IServiceCollection services)
    {
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IDiscountRepository, DiscountRepository>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<IDeliveryAddressRepository, DeliveryAddressRepository>();
        services.AddScoped<IDeliveryAddressService, DeliveryAddressService>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICloudService, CloudinaryService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoleRepository, UserRoleRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IValidator<UserCreateDto>, UserCreateDtoValidator>();
        services.AddScoped<IValidator<UserLoginDto>, UserLoginDtoValidator>();
        services.AddSingleton<Cloudinary>();
    }
}
