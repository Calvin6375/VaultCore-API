using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultCore.Application.DTOs;
using VaultCore.Application.Services;

namespace VaultCore.API.Controllers;

/// <summary>
/// User management and profile.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    /// <summary>Get current user profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetProfile(CancellationToken cancellationToken)
    {
        var user = await _userService.GetProfileAsync(cancellationToken);
        if (user == null) return NotFound();
        return Ok(user);
    }

    /// <summary>Update current user profile (or admin update any user).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateAsync(id, request, cancellationToken);
        if (user == null) return NotFound();
        return Ok(user);
    }

    /// <summary>Get user by ID (admin or self).</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Support")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user == null) return NotFound();
        return Ok(user);
    }

    /// <summary>Search users (admin).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Support")]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserDto>>> Search([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        var result = await _userService.SearchAsync(page, pageSize, search, cancellationToken);
        return Ok(result);
    }

    /// <summary>Assign role to user (admin only).</summary>
    [HttpPost("{id:guid}/roles")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> AssignRole(Guid id, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.AssignRoleAsync(id, request.RoleName, cancellationToken);
        if (user == null) return NotFound();
        return Ok(user);
    }
}
