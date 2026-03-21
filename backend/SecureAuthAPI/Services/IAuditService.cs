namespace SecureAuthAPI.Services;

public interface IAuditService
{
    Task LogAsync(int? userId, string action, string details, string ipAddress, bool success);
}