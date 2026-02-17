using Microsoft.EntityFrameworkCore;
using VaultCore.Domain.Entities;
using VaultCore.Domain.Enums;
using VaultCore.Domain.Interfaces;
using VaultCore.Infrastructure.Data;

namespace VaultCore.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context) => _context = context;

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Transactions.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default) =>
        await _context.Transactions.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetByWalletIdAsync(Guid walletId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Transactions.Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<int> CountByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default) =>
        await _context.Transactions.CountAsync(t => t.WalletId == walletId, cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetAllPagedAsync(int page, int pageSize, TransactionType? type = null, TransactionStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions.IgnoreQueryFilters().AsQueryable();
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);
        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        return await query.OrderByDescending(t => t.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAllAsync(TransactionType? type = null, TransactionStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions.IgnoreQueryFilters().AsQueryable();
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);
        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        return transaction;
    }

    public void Update(Transaction transaction) => _context.Transactions.Update(transaction);
}
