using IRouteCommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IRouteCommerce.Infrastructure.Persistence;

/// <summary>
/// Inicializa datos base de la aplicacion (usuario administrador para el login).
/// </summary>
public static class DatabaseInitializer
{
    public const string DefaultAdminUsername = "admin";
    public const string DefaultAdminPassword = "Admin123*";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();

        var exists = await context.AppUsers.AnyAsync(u => u.Username == DefaultAdminUsername);
        if (exists)
            return;

        var admin = new AppUser
        {
            Username = DefaultAdminUsername,
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, DefaultAdminPassword);

        context.AppUsers.Add(admin);
        await context.SaveChangesAsync();
    }
}
