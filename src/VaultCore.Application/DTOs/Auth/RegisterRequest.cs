namespace VaultCore.Application.DTOs.Auth;

/// <summary>
/// Request to register a new user.
/// </summary>
public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
);
