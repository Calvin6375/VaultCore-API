namespace VaultCore.Application.DTOs;

/// <summary>
/// Request to assign a role to a user (admin only).
/// </summary>
public record AssignRoleRequest(string RoleName);
