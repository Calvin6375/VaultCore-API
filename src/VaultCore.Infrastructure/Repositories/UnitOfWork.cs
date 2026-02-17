using VaultCore.Domain.Interfaces;
using VaultCore.Infrastructure.Data;

namespace VaultCore.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation using ApplicationDbContext.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(
        ApplicationDbContext context,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAuditLogRepository auditLogRepository)
    {
        _context = context;
        Users = userRepository;
        Roles = roleRepository;
        Wallets = walletRepository;
        Transactions = transactionRepository;
        RefreshTokens = refreshTokenRepository;
        AuditLogs = auditLogRepository;
    }

    public IUserRepository Users { get; }
    public IRoleRepository Roles { get; }
    public IWalletRepository Wallets { get; }
    public ITransactionRepository Transactions { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public IAuditLogRepository AuditLogs { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
