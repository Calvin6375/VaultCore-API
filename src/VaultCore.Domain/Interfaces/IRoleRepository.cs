using VaultCore.Domain.Entities;
using VaultCore.Domain.Enums;

namespace VaultCore.Domain.Interfaces;

/// <summary>
/// Role repository contract.
/// </summary>
public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(RoleType name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
}
