namespace IRouteCommerce.Application.DTOs;

/// <summary>Credenciales para el inicio de sesion.</summary>
public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
