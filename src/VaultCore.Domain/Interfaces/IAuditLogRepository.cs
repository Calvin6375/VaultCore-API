using VaultCore.Domain.Entities;

namespace VaultCore.Domain.Interfaces;

/// <summary>
/// Audit log repository contract.
/// </summary>
public interface IAuditLogRepository
{
    Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetPagedAsync(int page, int pageSize, Guid? userId = null, string? action = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid? userId = null, string? action = null, CancellationToken cancellationToken = default);
}
