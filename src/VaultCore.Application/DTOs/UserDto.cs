using VaultCore.Domain.Enums;

namespace VaultCore.Application.DTOs;

/// <summary>
/// User data transfer object.
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    KycStatus KycStatus,
    bool IsActive,
    bool IsFraudFlagged,
    IReadOnlyList<string> Roles,
    DateTime CreatedAtUtc
);
