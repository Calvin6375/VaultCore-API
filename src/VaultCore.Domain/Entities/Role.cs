using VaultCore.Domain.Common;
using VaultCore.Domain.Enums;

namespace VaultCore.Domain.Entities;

/// <summary>
/// Role entity for RBAC.
/// </summary>
public class Role : BaseEntity
{
    public RoleType Name { get; set; }
    public string Description { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
