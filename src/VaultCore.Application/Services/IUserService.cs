using VaultCore.Application.DTOs;

namespace VaultCore.Application.Services;

/// <summary>
/// User management operations.
/// </summary>
public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto?> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<UserDto>> SearchAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<UserDto?> AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
}
