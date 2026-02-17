namespace VaultCore.Application.DTOs.Auth;

/// <summary>
/// Authentication response with tokens and user info.
/// </summary>
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    UserDto User
);
