using IRouteCommerce.Application.Interfaces;
using IRouteCommerce.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IRouteCommerce.Application;

/// <summary>Registro de dependencias de la capa Application.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommerceService, CommerceService>();
        return services;
    }
}
