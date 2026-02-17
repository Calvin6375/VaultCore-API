using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultCore.Application.DTOs;
using VaultCore.Application.Services;
using VaultCore.Domain.Enums;

namespace VaultCore.API.Controllers;

/// <summary>
/// Transactions: deposit, withdrawal, transfer, history.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService) => _transactionService = transactionService;

    /// <summary>Deposit into a user's wallet (admin only).</summary>
    [HttpPost("deposit/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> Deposit(Guid userId, [FromBody] DepositRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tx = await _transactionService.DepositAsync(userId, request, cancellationToken);
            if (tx == null) return NotFound();
            return Ok(tx);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Withdraw from my wallet.</summary>
    [HttpPost("withdraw")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionDto>> Withdraw([FromBody] WithdrawalRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tx = await _transactionService.WithdrawAsync(request, cancellationToken);
            if (tx == null) return Unauthorized();
            return Ok(tx);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Transfer to another user.</summary>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionDto>> Transfer([FromBody] TransferRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tx = await _transactionService.TransferAsync(request, cancellationToken);
            if (tx == null) return Unauthorized();
            return Ok(tx);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get transaction by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tx = await _transactionService.GetByIdAsync(id, cancellationToken);
        if (tx == null) return NotFound();
        return Ok(tx);
    }

    /// <summary>Get my transaction history.</summary>
    [HttpGet("me/history")]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetMyHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        var result = await _transactionService.GetMyHistoryAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get all transactions (admin).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Support")]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] TransactionType? type = null, [FromQuery] TransactionStatus? status = null, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        var result = await _transactionService.GetAllPagedAsync(page, pageSize, type, status, cancellationToken);
        return Ok(result);
    }
}
