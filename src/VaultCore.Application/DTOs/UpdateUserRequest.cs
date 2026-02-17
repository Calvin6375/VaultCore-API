using VaultCore.Domain.Enums;

namespace VaultCore.Application.DTOs;

/// <summary>
/// Request to update user profile or admin fields.
/// </summary>
public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    KycStatus? KycStatus,
    bool? IsActive,
    bool? IsFraudFlagged
);
