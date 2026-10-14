using IRouteCommerce.Application.Interfaces;
using IRouteCommerce.Domain.Entities;
using IRouteCommerce.Infrastructure.Persistence;
using IRouteCommerce.Infrastructure.Repositories;
using IRouteCommerce.Infrastructure.Security;
using IRouteCommerce.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IRouteCommerce.Infrastructure;

/// <summary>Registro de dependencias de la capa Infrastructure.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CommerceDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICommerceRepository, CommerceRepository>();
        services.AddScoped<ICsvService, CsvService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        return services;
    }
}
