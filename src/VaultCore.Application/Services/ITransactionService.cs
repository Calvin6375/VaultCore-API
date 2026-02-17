using VaultCore.Application.DTOs;
using VaultCore.Domain.Enums;

namespace VaultCore.Application.Services;

/// <summary>
/// Transaction operations.
/// </summary>
public interface ITransactionService
{
    Task<TransactionDto?> DepositAsync(Guid walletUserId, DepositRequest request, CancellationToken cancellationToken = default);
    Task<TransactionDto?> WithdrawAsync(WithdrawalRequest request, CancellationToken cancellationToken = default);
    Task<TransactionDto?> TransferAsync(TransferRequest request, CancellationToken cancellationToken = default);
    Task<TransactionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<TransactionDto>> GetMyHistoryAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<TransactionDto>> GetAllPagedAsync(int page, int pageSize, TransactionType? type, TransactionStatus? status, CancellationToken cancellationToken = default);
}
