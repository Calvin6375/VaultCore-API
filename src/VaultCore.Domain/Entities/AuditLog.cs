namespace VaultCore.Domain.Entities;

/// <summary>
/// Audit log entry for sensitive actions.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? BeforeState { get; set; }
    public string? AfterState { get; set; }
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
}
