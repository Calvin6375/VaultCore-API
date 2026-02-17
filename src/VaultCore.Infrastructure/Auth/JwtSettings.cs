namespace VaultCore.Infrastructure.Auth;

/// <summary>
/// JWT configuration (bound from configuration).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "VaultCore";
    public string Audience { get; set; } = "VaultCore";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
