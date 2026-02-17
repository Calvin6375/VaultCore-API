namespace VaultCore.Application.DTOs;

/// <summary>
/// Audit log entry DTO.
/// </summary>
public record AuditLogDto(
    Guid Id,
    DateTime TimestampUtc,
    Guid? UserId,
    string? UserEmail,
    string Action,
    string EntityType,
    string? EntityId,
    string? BeforeState,
    string? AfterState,
    string? IpAddress,
    string? CorrelationId
);
