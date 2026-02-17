using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultCore.Application.DTOs;
using VaultCore.Application.Services;

namespace VaultCore.API.Controllers;

/// <summary>
/// Admin dashboard APIs: users, transactions, audit logs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITransactionService _transactionService;
    private readonly IAuditQueryService _auditQueryService;

    public AdminController(IUserService userService, ITransactionService transactionService, IAuditQueryService auditQueryService)
    {
        _userService = userService;
        _transactionService = transactionService;
        _auditQueryService = auditQueryService;
    }

    /// <summary>Get all users (paginated).</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        var result = await _userService.SearchAsync(page, pageSize, search, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get audit logs (paginated).</summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? userId = null, [FromQuery] string? action = null, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        var result = await _auditQueryService.GetPagedAsync(page, pageSize, userId, action, cancellationToken);
        return Ok(result);
    }
}
