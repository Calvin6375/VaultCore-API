namespace VaultCore.Application.DTOs;

/// <summary>
/// Request to deposit into a wallet.
/// </summary>
public record DepositRequest(decimal Amount, string? IdempotencyKey = null, string? Reference = null);
