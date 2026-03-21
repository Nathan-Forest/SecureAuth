using SecureAuthAPI.DTOs;

namespace SecureAuthAPI.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress);
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress);
    Task<AuthResponse> RefreshTokenAsync(string token, string ipAddress);
    Task<bool> RevokeTokenAsync(string token, string ipAddress);
    Task<bool> VerifyTokenAsync(string token);
}