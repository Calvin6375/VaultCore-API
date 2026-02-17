using AutoMapper;
using Microsoft.Extensions.Logging;
using VaultCore.Application.Common;
using VaultCore.Application.DTOs;
using VaultCore.Domain.Entities;
using VaultCore.Domain.Enums;
using VaultCore.Domain.Interfaces;

namespace VaultCore.Application.Services;

/// <summary>
/// Transactions: deposit, withdrawal, transfer with idempotency support.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IMapper mapper,
        ILogger<TransactionService> logger)
    {
        _uow = uow;
        _currentUser = currentUser;
        _auditService = auditService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionDto?> DepositAsync(Guid walletUserId, DepositRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Roles.Contains("Admin"))
            return null;

        var wallet = await _uow.Wallets.GetByUserIdAsync(walletUserId, cancellationToken);
        if (wallet == null || wallet.IsDeleted) return null;
        if (wallet.Status != WalletStatus.Active)
            throw new InvalidOperationException("Wallet is not active for deposits.");

        var idempotencyKey = request.IdempotencyKey?.Trim();
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existing = await _uow.Transactions.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
            if (existing != null)
                return _mapper.Map<TransactionDto>(existing);
        }

        var balanceBefore = wallet.Balance;
        wallet.Balance += request.Amount;
        wallet.UpdatedAtUtc = DateTime.UtcNow;
        _uow.Wallets.Update(wallet);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Completed,
            Amount = request.Amount,
            CurrencyCode = wallet.CurrencyCode,
            Reference = request.Reference,
            BalanceBefore = balanceBefore,
            BalanceAfter = wallet.Balance,
            IdempotencyKey = idempotencyKey,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _uow.Transactions.AddAsync(transaction, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Transaction.Deposit", "Transaction", transaction.Id.ToString(), beforeState: new { balanceBefore }, afterState: new { transaction.BalanceAfter }, cancellationToken);
        return _mapper.Map<TransactionDto>(transaction);
    }

    public async Task<TransactionDto?> WithdrawAsync(WithdrawalRequest request, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId == null) return null;

        var wallet = await _uow.Wallets.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (wallet == null || wallet.IsDeleted) return null;
        if (wallet.Status != WalletStatus.Active)
            throw new InvalidOperationException("Wallet is not active for withdrawals.");

        var idempotencyKey = request.IdempotencyKey?.Trim();
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existing = await _uow.Transactions.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
            if (existing != null)
                return _mapper.Map<TransactionDto>(existing);
        }

        if (wallet.Balance < request.Amount)
            throw new InvalidOperationException("Insufficient balance.");

        var balanceBefore = wallet.Balance;
        wallet.Balance -= request.Amount;
        wallet.UpdatedAtUtc = DateTime.UtcNow;
        _uow.Wallets.Update(wallet);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Type = TransactionType.Withdrawal,
            Status = TransactionStatus.Completed,
            Amount = -request.Amount,
            CurrencyCode = wallet.CurrencyCode,
            Reference = request.Reference,
            BalanceBefore = balanceBefore,
            BalanceAfter = wallet.Balance,
            IdempotencyKey = idempotencyKey,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _uow.Transactions.AddAsync(transaction, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Transaction.Withdrawal", "Transaction", transaction.Id.ToString(), beforeState: new { balanceBefore }, afterState: new { transaction.BalanceAfter }, cancellationToken);
        return _mapper.Map<TransactionDto>(transaction);
    }

    public async Task<TransactionDto?> TransferAsync(TransferRequest request, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId == null) return null;

        var fromWallet = await _uow.Wallets.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (fromWallet == null || fromWallet.IsDeleted) return null;
        if (fromWallet.Status != WalletStatus.Active)
            throw new InvalidOperationException("Your wallet is not active.");

        var toUser = await _uow.Users.GetByIdAsync(request.ToUserId, includeWallet: true, cancellationToken: cancellationToken);
        var toWallet = toUser?.Wallet;
        if (toWallet == null || toWallet.IsDeleted)
            throw new InvalidOperationException("Recipient wallet not found.");
        if (toWallet.Status != WalletStatus.Active)
            throw new InvalidOperationException("Recipient wallet is not active.");
        if (fromWallet.Id == toWallet.Id)
            throw new InvalidOperationException("Cannot transfer to self.");

        if (fromWallet.Balance < request.Amount)
            throw new InvalidOperationException("Insufficient balance.");

        var idempotencyKey = request.IdempotencyKey?.Trim();
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existing = await _uow.Transactions.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
            if (existing != null)
                return _mapper.Map<TransactionDto>(existing);
        }

        var fromBalanceBefore = fromWallet.Balance;
        var toBalanceBefore = toWallet.Balance;
        fromWallet.Balance -= request.Amount;
        toWallet.Balance += request.Amount;
        fromWallet.UpdatedAtUtc = DateTime.UtcNow;
        toWallet.UpdatedAtUtc = DateTime.UtcNow;
        _uow.Wallets.Update(fromWallet);
        _uow.Wallets.Update(toWallet);

        var debitTx = new Transaction
        {
            Id = Guid.NewGuid(),
            WalletId = fromWallet.Id,
            Type = TransactionType.Transfer,
            Status = TransactionStatus.Completed,
            Amount = -request.Amount,
            CurrencyCode = fromWallet.CurrencyCode,
            CounterpartyWalletId = toWallet.Id,
            Reference = request.Reference,
            Description = request.Description ?? $"Transfer to {toUser!.Email}",
            BalanceBefore = fromBalanceBefore,
            BalanceAfter = fromWallet.Balance,
            IdempotencyKey = idempotencyKey,
            CreatedAtUtc = DateTime.UtcNow
        };
        var creditTx = new Transaction
        {
            Id = Guid.NewGuid(),
            WalletId = toWallet.Id,
            Type = TransactionType.Transfer,
            Status = TransactionStatus.Completed,
            Amount = request.Amount,
            CurrencyCode = toWallet.CurrencyCode,
            CounterpartyWalletId = fromWallet.Id,
            Reference = request.Reference,
            Description = request.Description ?? $"Transfer from {_currentUser.Email}",
            BalanceBefore = toBalanceBefore,
            BalanceAfter = toWallet.Balance,
            IdempotencyKey = idempotencyKey,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _uow.Transactions.AddAsync(debitTx, cancellationToken);
        await _uow.Transactions.AddAsync(creditTx, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Transaction.Transfer", "Transaction", debitTx.Id.ToString(), beforeState: new { fromBalanceBefore, toBalanceBefore }, afterState: new { debitTx.BalanceAfter, creditTx.BalanceAfter }, cancellationToken);
        return _mapper.Map<TransactionDto>(debitTx);
    }

    public async Task<TransactionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tx = await _uow.Transactions.GetByIdAsync(id, cancellationToken);
        if (tx == null) return null;
        var wallet = await _uow.Wallets.GetByIdAsync(tx.WalletId, cancellationToken);
        if (wallet != null && _currentUser.UserId != wallet.UserId && !_currentUser.Roles.Contains("Admin"))
            return null;
        return _mapper.Map<TransactionDto>(tx);
    }

    public async Task<PagedResult<TransactionDto>> GetMyHistoryAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId == null)
            return new PagedResult<TransactionDto>(Array.Empty<TransactionDto>(), 0, page, pageSize);

        var wallet = await _uow.Wallets.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (wallet == null)
            return new PagedResult<TransactionDto>(Array.Empty<TransactionDto>(), 0, page, pageSize);

        var total = await _uow.Transactions.CountByWalletIdAsync(wallet.Id, cancellationToken);
        var list = await _uow.Transactions.GetByWalletIdAsync(wallet.Id, page, pageSize, cancellationToken);
        return new PagedResult<TransactionDto>(list.Select(_mapper.Map<TransactionDto>).ToList(), total, page, pageSize);
    }

    public async Task<PagedResult<TransactionDto>> GetAllPagedAsync(int page, int pageSize, TransactionType? type, TransactionStatus? status, CancellationToken cancellationToken = default)
    {
        var total = await _uow.Transactions.CountAllAsync(type, status, cancellationToken);
        var list = await _uow.Transactions.GetAllPagedAsync(page, pageSize, type, status, cancellationToken);
        return new PagedResult<TransactionDto>(list.Select(_mapper.Map<TransactionDto>).ToList(), total, page, pageSize);
    }
}
