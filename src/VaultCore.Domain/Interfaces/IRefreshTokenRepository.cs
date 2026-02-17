using VaultCore.Domain.Entities;

namespace VaultCore.Domain.Interfaces;

/// <summary>
/// Refresh token repository contract.
/// </summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<RefreshToken> AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    void Update(RefreshToken refreshToken);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
