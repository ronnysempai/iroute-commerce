using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IRouteCommerce.Application.DTOs;
using IRouteCommerce.Application.Interfaces;
using IRouteCommerce.Domain.Entities;
using IRouteCommerce.Infrastructure.Persistence;
using IRouteCommerce.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IRouteCommerce.Infrastructure.Services;

/// <summary>
/// Autenticacion basada en usuario/contrasena (hash PBKDF2 via PasswordHasher)
/// que emite un token JWT firmado con HMAC-SHA256.
/// </summary>
public class AuthService : IAuthService
{
    private readonly CommerceDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        CommerceDbContext context,
        IPasswordHasher<AppUser> passwordHasher,
        IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return null;

        var user = await _context.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user is null)
            return null;

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
            return null;

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        return new LoginResponseDto
        {
            Token = GenerateToken(user, expiresAt),
            Username = user.Username,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }

    private string GenerateToken(AppUser user, DateTime expiresAt)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
