using VaultCore.Domain.Entities;
using VaultCore.Domain.Enums;

namespace VaultCore.Domain.Interfaces;

/// <summary>
/// Transaction repository contract.
/// </summary>
public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetByWalletIdAsync(Guid walletId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetAllPagedAsync(int page, int pageSize, TransactionType? type = null, TransactionStatus? status = null, CancellationToken cancellationToken = default);
    Task<int> CountAllAsync(TransactionType? type = null, TransactionStatus? status = null, CancellationToken cancellationToken = default);
    Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    void Update(Transaction transaction);
}
