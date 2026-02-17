namespace VaultCore.Domain.Enums;

/// <summary>
/// KYC verification status for users.
/// </summary>
public enum KycStatus
{
    Pending = 0,
    InReview = 1,
    Verified = 2,
    Rejected = 3
}
