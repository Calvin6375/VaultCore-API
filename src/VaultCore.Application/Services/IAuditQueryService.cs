using VaultCore.Application.DTOs;

namespace VaultCore.Application.Services;

/// <summary>
/// Query audit logs (admin).
/// </summary>
public interface IAuditQueryService
{
    Task<PagedResult<AuditLogDto>> GetPagedAsync(int page, int pageSize, Guid? userId, string? action, CancellationToken cancellationToken = default);
}
