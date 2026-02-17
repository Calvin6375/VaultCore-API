namespace VaultCore.Application.DTOs;

/// <summary>
/// Request to transfer between two users' wallets.
/// </summary>
public record TransferRequest(
    Guid ToUserId,
    decimal Amount,
    string? IdempotencyKey = null,
    string? Reference = null,
    string? Description = null
);
