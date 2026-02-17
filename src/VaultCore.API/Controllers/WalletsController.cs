using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultCore.Application.DTOs;
using VaultCore.Application.Services;

namespace VaultCore.API.Controllers;

/// <summary>
/// Wallet balance and admin freeze/unfreeze.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService) => _walletService = walletService;

    /// <summary>Get my wallet balance.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletDto>> GetMyWallet(CancellationToken cancellationToken)
    {
        var wallet = await _walletService.GetMyWalletAsync(cancellationToken);
        if (wallet == null) return NotFound();
        return Ok(wallet);
    }

    /// <summary>Get wallet by user ID (admin/support).</summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = "Admin,Support")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletDto>> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var wallet = await _walletService.GetByUserIdAsync(userId, cancellationToken);
        if (wallet == null) return NotFound();
        return Ok(wallet);
    }

    /// <summary>Freeze wallet (admin only).</summary>
    [HttpPost("{walletId:guid}/freeze")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletDto>> Freeze(Guid walletId, CancellationToken cancellationToken)
    {
        var wallet = await _walletService.FreezeAsync(walletId, cancellationToken);
        if (wallet == null) return NotFound();
        return Ok(wallet);
    }

    /// <summary>Unfreeze wallet (admin only).</summary>
    [HttpPost("{walletId:guid}/unfreeze")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletDto>> Unfreeze(Guid walletId, CancellationToken cancellationToken)
    {
        var wallet = await _walletService.UnfreezeAsync(walletId, cancellationToken);
        if (wallet == null) return NotFound();
        return Ok(wallet);
    }
}
