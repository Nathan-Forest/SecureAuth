using System.ComponentModel.DataAnnotations;

namespace SecureAuthAPI.Models;

public class AuditLog
{
    public int Id { get; set; }
    
    public int? UserId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;  // LOGIN, LOGOUT, REGISTER, etc.
    
    [MaxLength(500)]
    public string Details { get; set; } = string.Empty;
    
    [MaxLength(45)]
    public string IpAddress { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string UserAgent { get; set; } = string.Empty;
    
    public bool Success { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual User? User { get; set; }
}