using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SecureAuthAPI.Data;
using SecureAuthAPI.DTOs;
using SecureAuthAPI.Models;

namespace SecureAuthAPI.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IAuditService _auditService;
    
    public AuthService(AppDbContext context, IConfiguration configuration, IAuditService auditService)
    {
        _context = context;
        _configuration = configuration;
        _auditService = auditService;
    }
    
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress)
    {
        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            await _auditService.LogAsync(null, "REGISTER_FAILED", 
                $"Email already exists: {request.Email}", ipAddress, false);
            throw new InvalidOperationException("Email already registered");
        }
        
        // Hash password using BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);
        
        // Create user
        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = passwordHash,
            EmailVerified = false,  // In production, send verification email
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // Log success
        await _auditService.LogAsync(user.Id, "REGISTER_SUCCESS", 
            $"User registered: {user.Email}", ipAddress, true);
        
        // Generate tokens
        var jwtToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken(user.Id, ipAddress);
        
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        
        return new AuthResponse
        {
            Token = jwtToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = MapToUserDto(user)
        };
    }
    
    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
        
        if (user == null)
        {
            await _auditService.LogAsync(null, "LOGIN_FAILED", 
                $"User not found: {request.Email}", ipAddress, false);
            throw new UnauthorizedAccessException("Invalid email or password");
        }
        
        // Check if account is locked
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            await _auditService.LogAsync(user.Id, "LOGIN_FAILED", 
                "Account locked", ipAddress, false);
            throw new UnauthorizedAccessException("Account is locked. Try again later.");
        }
        
        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // Increment failed attempts
            user.FailedLoginAttempts++;
            
            // Lock account after 5 failed attempts
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                await _auditService.LogAsync(user.Id, "ACCOUNT_LOCKED", 
                    "Too many failed login attempts", ipAddress, false);
            }
            
            await _context.SaveChangesAsync();
            
            await _auditService.LogAsync(user.Id, "LOGIN_FAILED", 
                "Invalid password", ipAddress, false);
            throw new UnauthorizedAccessException("Invalid email or password");
        }
        
        // Check if account is active
        if (!user.IsActive)
        {
            await _auditService.LogAsync(user.Id, "LOGIN_FAILED", 
                "Account inactive", ipAddress, false);
            throw new UnauthorizedAccessException("Account is inactive");
        }
        
        // Reset failed attempts on successful login
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        
        // Generate tokens
        var jwtToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken(user.Id, ipAddress);
        
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        
        await _auditService.LogAsync(user.Id, "LOGIN_SUCCESS", 
            $"User logged in: {user.Email}", ipAddress, true);
        
        return new AuthResponse
        {
            Token = jwtToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = MapToUserDto(user)
        };
    }
    
    public async Task<AuthResponse> RefreshTokenAsync(string token, string ipAddress)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
        
        if (refreshToken == null || !refreshToken.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }
        
        // Generate new tokens
        var newJwtToken = GenerateJwtToken(refreshToken.User);
        var newRefreshToken = GenerateRefreshToken(refreshToken.User.Id, ipAddress);
        
        // Revoke old refresh token
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();
        
        return new AuthResponse
        {
            Token = newJwtToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = MapToUserDto(refreshToken.User)
        };
    }
    
    public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);
        
        if (refreshToken == null || !refreshToken.IsActive)
            return false;
        
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> VerifyTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured"));
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // Private helper methods
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured"));
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("email_verified", user.EmailVerified.ToString()),
            new Claim("2fa_enabled", user.TwoFactorEnabled.ToString())
        };
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),  // Short-lived JWT
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    private RefreshToken GenerateRefreshToken(int userId, string ipAddress)
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[64];
        rng.GetBytes(randomBytes);
        
        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),  // Long-lived refresh token
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }
    
    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            EmailVerified = user.EmailVerified,
            TwoFactorEnabled = user.TwoFactorEnabled,
            CreatedAt = user.CreatedAt
        };
    }
}