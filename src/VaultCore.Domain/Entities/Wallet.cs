using VaultCore.Domain.Common;
using VaultCore.Domain.Enums;

namespace VaultCore.Domain.Entities;

/// <summary>
/// Wallet account for a user. One user has one wallet (MVP).
/// </summary>
public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string CurrencyCode { get; set; } = "KES";
    public decimal Balance { get; set; }
    public WalletStatus Status { get; set; } = WalletStatus.Active;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
