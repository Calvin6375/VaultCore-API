using AutoMapper;
using Microsoft.Extensions.Logging;
using VaultCore.Application.Common;
using VaultCore.Application.DTOs;
using VaultCore.Domain.Enums;
using VaultCore.Domain.Interfaces;

namespace VaultCore.Application.Services;

/// <summary>
/// Wallet operations: get balance, freeze/unfreeze (admin).
/// </summary>
public class WalletService : IWalletService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IMapper mapper,
        ILogger<WalletService> logger)
    {
        _uow = uow;
        _currentUser = currentUser;
        _auditService = auditService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<WalletDto?> GetMyWalletAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId == null) return null;
        var wallet = await _uow.Wallets.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        return wallet == null ? null : _mapper.Map<WalletDto>(wallet);
    }

    public async Task<WalletDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await _uow.Wallets.GetByUserIdAsync(userId, cancellationToken);
        if (wallet == null) return null;
        if (!_currentUser.Roles.Contains("Admin") && _currentUser.UserId != userId)
            return null;
        return _mapper.Map<WalletDto>(wallet);
    }

    public async Task<WalletDto?> FreezeAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Roles.Contains("Admin"))
            return null;

        var wallet = await _uow.Wallets.GetByIdAsync(walletId, cancellationToken);
        if (wallet == null || wallet.IsDeleted) return null;

        var before = wallet.Status;
        wallet.Status = WalletStatus.Frozen;
        wallet.UpdatedAtUtc = DateTime.UtcNow;
        _uow.Wallets.Update(wallet);
        await _uow.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Wallet.Frozen", "Wallet", walletId.ToString(), beforeState: new { Status = before.ToString() }, afterState: new { Status = "Frozen" }, cancellationToken);
        _logger.LogWarning("Wallet {WalletId} frozen by admin", walletId);
        return _mapper.Map<WalletDto>(wallet);
    }

    public async Task<WalletDto?> UnfreezeAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Roles.Contains("Admin"))
            return null;

        var wallet = await _uow.Wallets.GetByIdAsync(walletId, cancellationToken);
        if (wallet == null || wallet.IsDeleted) return null;

        var before = wallet.Status;
        wallet.Status = WalletStatus.Active;
        wallet.UpdatedAtUtc = DateTime.UtcNow;
        _uow.Wallets.Update(wallet);
        await _uow.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("Wallet.Unfrozen", "Wallet", walletId.ToString(), beforeState: new { Status = before.ToString() }, afterState: new { Status = "Active" }, cancellationToken);
        return _mapper.Map<WalletDto>(wallet);
    }
}
