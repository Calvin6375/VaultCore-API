using VaultCore.Domain.Entities;

namespace VaultCore.Application.Common;

/// <summary>
/// JWT and refresh token generation/validation.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(User user, IReadOnlyList<string> roles);
    string GenerateRefreshToken();
    Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, string ipAddress, CancellationToken cancellationToken = default);
}
