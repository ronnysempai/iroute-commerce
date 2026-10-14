namespace IRouteCommerce.Domain.Entities;

/// <summary>
/// Usuario de la aplicacion utilizado para el inicio de sesion (JWT).
/// Corresponde a la tabla dbo.app_users.
/// </summary>
public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; }
}
