using VaultCore.Domain.Common;
using VaultCore.Domain.Enums;

namespace VaultCore.Domain.Entities;

/// <summary>
/// User account entity.
/// </summary>
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public KycStatus KycStatus { get; set; } = KycStatus.Pending;
    public bool IsActive { get; set; } = true;
    public bool IsFraudFlagged { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public Wallet? Wallet { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
