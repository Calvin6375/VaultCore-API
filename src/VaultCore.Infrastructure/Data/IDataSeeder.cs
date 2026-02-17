namespace VaultCore.Infrastructure.Data;

/// <summary>
/// Seeds initial roles and admin user.
/// </summary>
public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
