namespace VaultCore.Application.DTOs.Auth;

/// <summary>
/// Request to login.
/// </summary>
public record LoginRequest(string Email, string Password);
