using VaultCore.Domain.Enums;

namespace VaultCore.Application.DTOs;

/// <summary>
/// Wallet data transfer object.
/// </summary>
public record WalletDto(
    Guid Id,
    Guid UserId,
    string CurrencyCode,
    decimal Balance,
    WalletStatus Status,
    DateTime CreatedAtUtc
);
