using AutoMapper;
using Microsoft.Extensions.Logging;
using VaultCore.Application.Common;
using VaultCore.Application.DTOs;
using VaultCore.Domain.Enums;
using VaultCore.Domain.Interfaces;

namespace VaultCore.Application.Services;

/// <summary>
/// User management: profile, update, search, role assignment.
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _uow = uow;
        _currentUser = currentUser;
        _auditService = auditService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, includeRoles: true, cancellationToken: cancellationToken);
        return user == null || user.IsDeleted ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId == null) return null;
        return await GetByIdAsync(_currentUser.UserId.Value, cancellationToken);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, includeRoles: true, cancellationToken: cancellationToken);
        if (user == null || user.IsDeleted) return null;

        // Non-admin can only update own profile and only certain fields
        var isAdmin = _currentUser.Roles.Contains("Admin");
        if (!isAdmin && _currentUser.UserId != id)
            return null;

        var before = new { user.FirstName, user.LastName, user.PhoneNumber, user.KycStatus, user.IsActive, user.IsFraudFlagged };
        if (request.FirstName != null) user.FirstName = request.FirstName.Trim();
        if (request.LastName != null) user.LastName = request.LastName.Trim();
        if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber.Trim();
        if (request.KycStatus.HasValue && isAdmin) user.KycStatus = request.KycStatus.Value;
        if (request.IsActive.HasValue && isAdmin) user.IsActive = request.IsActive.Value;
        if (request.IsFraudFlagged.HasValue && isAdmin) user.IsFraudFlagged = request.IsFraudFlagged.Value;
        user.UpdatedAtUtc = DateTime.UtcNow;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("User.Updated", "User", id.ToString(), beforeState: before, afterState: new { user.FirstName, user.LastName, user.KycStatus, user.IsActive, user.IsFraudFlagged }, cancellationToken);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<PagedResult<UserDto>> SearchAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var total = await _uow.Users.CountAsync(search, includeDeleted: false, cancellationToken);
        var users = await _uow.Users.GetPagedAsync(page, pageSize, search, includeDeleted: false, cancellationToken);
        var dtos = users.Select(u => _mapper.Map<UserDto>(u)).ToList();
        return new PagedResult<UserDto>(dtos, total, page, pageSize);
    }

    public async Task<UserDto?> AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Roles.Contains("Admin"))
            return null;

        var user = await _uow.Users.GetByIdAsync(userId, includeRoles: true, cancellationToken: cancellationToken);
        if (user == null || user.IsDeleted) return null;

        var roleType = Enum.Parse<RoleType>(roleName, ignoreCase: true);
        var role = await _uow.Roles.GetByNameAsync(roleType, cancellationToken);
        if (role == null) return null;

        if (user.UserRoles.Any(ur => ur.RoleId == role.Id))
            return _mapper.Map<UserDto>(user);

        user.UserRoles.Add(new Domain.Entities.UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAtUtc = DateTime.UtcNow
        });
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("User.RoleAssigned", "User", userId.ToString(), afterState: new { Role = roleName }, cancellationToken);
        _logger.LogInformation("Role {Role} assigned to user {UserId}", roleName, userId);
        return _mapper.Map<UserDto>(user);
    }
}
