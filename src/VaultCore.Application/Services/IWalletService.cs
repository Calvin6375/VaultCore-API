using VaultCore.Application.DTOs;

namespace VaultCore.Application.Services;

/// <summary>
/// Wallet operations.
/// </summary>
public interface IWalletService
{
    Task<WalletDto?> GetMyWalletAsync(CancellationToken cancellationToken = default);
    Task<WalletDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WalletDto?> FreezeAsync(Guid walletId, CancellationToken cancellationToken = default);
    Task<WalletDto?> UnfreezeAsync(Guid walletId, CancellationToken cancellationToken = default);
}
