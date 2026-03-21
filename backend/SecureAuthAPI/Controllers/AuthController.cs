using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureAuthAPI.DTOs;
using SecureAuthAPI.Services;

namespace SecureAuthAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await _authService.RegisterAsync(request, ipAddress);
            
            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            
            return CreatedAtAction(nameof(Register), new { email = request.Email }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { error = "An error occurred during registration" });
        }
    }
    
    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await _authService.LoginAsync(request, ipAddress);
            
            // Set refresh token in HTTP-only cookie
            SetRefreshTokenCookie(response.RefreshToken);
            
            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed: {Message}", ex.Message);
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "An error occurred during login" });
        }
    }
    
    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { error = "Refresh token is required" });
            }
            
            var ipAddress = GetIpAddress();
            var response = await _authService.RefreshTokenAsync(refreshToken, ipAddress);
            
            // Set new refresh token in cookie
            SetRefreshTokenCookie(response.RefreshToken);
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { error = "An error occurred during token refresh" });
        }
    }
    
    /// <summary>
    /// Logout and revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var ipAddress = GetIpAddress();
                await _authService.RevokeTokenAsync(refreshToken, ipAddress);
            }
            
            // Remove refresh token cookie
            Response.Cookies.Delete("refreshToken");
            
            _logger.LogInformation("User logged out successfully");
            
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { error = "An error occurred during logout" });
        }
    }
    
    /// <summary>
    /// Verify if a JWT token is valid
    /// </summary>
    [HttpGet("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyToken([FromHeader(Name = "Authorization")] string? authorization)
    {
        try
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return Unauthorized(new { valid = false, error = "Invalid authorization header" });
            }
            
            var token = authorization.Substring("Bearer ".Length).Trim();
            var isValid = await _authService.VerifyTokenAsync(token);
            
            if (!isValid)
            {
                return Unauthorized(new { valid = false, error = "Invalid token" });
            }
            
            // Extract claims from token for response
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
            
            return Ok(new 
            { 
                valid = true, 
                user = new 
                {
                    id = userId,
                    email = email,
                    name = name
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Token verification failed: {Message}", ex.Message);
            return Unauthorized(new { valid = false, error = "Token verification failed" });
        }
    }
    
    // Helper methods
    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"].ToString();
        }
        
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
    }
    
    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,  // Prevent JavaScript access (XSS protection)
            Secure = true,    // HTTPS only
            SameSite = SameSiteMode.Strict,  // CSRF protection
            Expires = DateTime.UtcNow.AddDays(7)
        };
        
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
