using VaultCore.Domain.Enums;

namespace VaultCore.Application.DTOs;

/// <summary>
/// Transaction data transfer object.
/// </summary>
public record TransactionDto(
    Guid Id,
    Guid WalletId,
    TransactionType Type,
    TransactionStatus Status,
    decimal Amount,
    string CurrencyCode,
    Guid? CounterpartyWalletId,
    string? Reference,
    string? Description,
    decimal BalanceBefore,
    decimal BalanceAfter,
    DateTime CreatedAtUtc
);
