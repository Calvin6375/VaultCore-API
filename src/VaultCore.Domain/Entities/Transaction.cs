using VaultCore.Domain.Common;
using VaultCore.Domain.Enums;

namespace VaultCore.Domain.Entities;

/// <summary>
/// Financial transaction (deposit, withdrawal, transfer).
/// </summary>
public class Transaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;

    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "KES";

    /// <summary>Optional: for transfers, the counterparty wallet.</summary>
    public Guid? CounterpartyWalletId { get; set; }
    public Wallet? CounterpartyWallet { get; set; }

    /// <summary>Idempotency key to prevent duplicate processing.</summary>
    public string? IdempotencyKey { get; set; }

    public string? Reference { get; set; }
    public string? Description { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
}
