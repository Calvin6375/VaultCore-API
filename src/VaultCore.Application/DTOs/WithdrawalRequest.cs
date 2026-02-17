namespace VaultCore.Application.DTOs;

/// <summary>
/// Request to withdraw from a wallet.
/// </summary>
public record WithdrawalRequest(decimal Amount, string? IdempotencyKey = null, string? Reference = null);
