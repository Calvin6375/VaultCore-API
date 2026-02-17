using Microsoft.EntityFrameworkCore;
using VaultCore.Domain.Entities;
using VaultCore.Domain.Interfaces;
using VaultCore.Infrastructure.Data;

namespace VaultCore.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _context;

    public WalletRepository(ApplicationDbContext context) => _context = context;

    public async Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Wallets.IgnoreQueryFilters().FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

    public async Task<Wallet> AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        await _context.Wallets.AddAsync(wallet, cancellationToken);
        return wallet;
    }

    public void Update(Wallet wallet) => _context.Wallets.Update(wallet);
}
