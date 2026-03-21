using SecureAuthAPI.Data;
using SecureAuthAPI.Models;

namespace SecureAuthAPI.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    
    public AuditService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task LogAsync(int? userId, string action, string details, string ipAddress, bool success)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = string.Empty,  // Will be populated from HTTP context in controller
            Success = success,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}