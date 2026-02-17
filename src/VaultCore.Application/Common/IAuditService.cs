namespace VaultCore.Application.Common;

/// <summary>
/// Service to record audit logs for sensitive actions.
/// </summary>
public interface IAuditService
{
    Task LogAsync(string action, string entityType, string? entityId, object? beforeState = null, object? afterState = null, CancellationToken cancellationToken = default);
}
