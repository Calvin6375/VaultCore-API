using System.Text.Json;
using Microsoft.AspNetCore.Http;
using VaultCore.Application.Common;
using VaultCore.Domain.Entities;
using VaultCore.Domain.Interfaces;

namespace VaultCore.Infrastructure.Audit;

/// <summary>
/// Records audit log entries with current user and IP.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IUnitOfWork uow, ICurrentUserService currentUser, IHttpContextAccessor httpContextAccessor)
    {
        _uow = uow;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string entityType, string? entityId, object? beforeState = null, object? afterState = null, CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var ip = httpContext?.Connection?.RemoteIpAddress?.ToString();
        var correlationId = httpContext?.TraceIdentifier;
        var entry = new AuditLog
        {
            Id = Guid.NewGuid(),
            TimestampUtc = DateTime.UtcNow,
            UserId = _currentUser.UserId,
            UserEmail = _currentUser.Email,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            BeforeState = beforeState == null ? null : JsonSerializer.Serialize(beforeState),
            AfterState = afterState == null ? null : JsonSerializer.Serialize(afterState),
            IpAddress = ip,
            CorrelationId = correlationId
        };
        await _uow.AuditLogs.AddAsync(entry, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
