namespace VaultCore.Application.DTOs.Auth;

/// <summary>
/// Request to refresh access token.
/// </summary>
public record RefreshTokenRequest(string RefreshToken);
