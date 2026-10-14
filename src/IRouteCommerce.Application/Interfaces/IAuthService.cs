using IRouteCommerce.Application.DTOs;

namespace IRouteCommerce.Application.Interfaces;

/// <summary>Servicio de inicio de sesion (emision de token JWT).</summary>
public interface IAuthService
{
    /// <summary>Valida las credenciales y devuelve el token, o null si son invalidas.</summary>
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}
