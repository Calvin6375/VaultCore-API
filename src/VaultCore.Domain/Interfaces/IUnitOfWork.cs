using VaultCore.Domain.Entities;

namespace VaultCore.Domain.Interfaces;

/// <summary>
/// Unit of Work for transactional operations across repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IWalletRepository Wallets { get; }
    ITransactionRepository Transactions { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IAuditLogRepository AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
