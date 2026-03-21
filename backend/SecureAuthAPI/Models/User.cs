using System.ComponentModel.DataAnnotations;

namespace SecureAuthAPI.Models;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public bool EmailVerified { get; set; } = false;
    
    public bool TwoFactorEnabled { get; set; } = false;
    
    public string? TwoFactorSecret { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public int FailedLoginAttempts { get; set; } = 0;
    
    public DateTime? LockoutEnd { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}