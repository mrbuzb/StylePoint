using AutoLedger.Infrastructure.Persistence.Repositories;
using CloudinaryDotNet;
using FluentValidation;
using StylePoint.Application.Dtos;
using StylePoint.Application.Helpers;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Implementations;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Application.Validators;

namespace AutoLedger.Api.Configurations;

public static class DependecyInjectionsConfiguration
{
    public static void ConfigureDependecies(this IServiceCollection services)
    {
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
