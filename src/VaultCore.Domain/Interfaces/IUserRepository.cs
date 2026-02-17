using VaultCore.Domain.Entities;

namespace VaultCore.Domain.Interfaces;

/// <summary>
/// User repository contract.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, bool includeRoles = false, bool includeWallet = false, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, bool includeRoles = false, bool includeWallet = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetPagedAsync(int page, int pageSize, string? search = null, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search = null, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
}
