using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VaultCore.Application.Common;
using VaultCore.Domain.Entities;
using VaultCore.Domain.Enums;

namespace VaultCore.Infrastructure.Data;

/// <summary>
/// Seeds roles (Admin, Customer, Support) and initial admin user.
/// </summary>
public class DataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(ApplicationDbContext context, IPasswordHasher passwordHasher, ILogger<DataSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedAdminUserAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        foreach (RoleType roleType in Enum.GetValues(typeof(RoleType)))
        {
            var exists = await _context.Roles.IgnoreQueryFilters().AnyAsync(r => r.Name == roleType, cancellationToken);
            if (exists) continue;
            _context.Roles.Add(new Role
            {
                Id = Guid.NewGuid(),
                Name = roleType,
                Description = $"{roleType} role",
                CreatedAtUtc = DateTime.UtcNow
            });
            _logger.LogInformation("Seeded role: {Role}", roleType);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        const string adminEmail = "admin@vaultcore.local";
        if (await _context.Users.AnyAsync(u => u.Email == adminEmail, cancellationToken))
            return;

        var adminRole = await _context.Roles.IgnoreQueryFilters().FirstAsync(r => r.Name == RoleType.Admin, cancellationToken);
        var adminId = Guid.NewGuid();
        var admin = new User
        {
            Id = adminId,
            Email = adminEmail,
            PasswordHash = _passwordHasher.Hash("Admin@123"), // Change in production
            FirstName = "System",
            LastName = "Admin",
            KycStatus = KycStatus.Verified,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.Users.Add(admin);
        _context.UserRoles.Add(new UserRole { UserId = adminId, RoleId = adminRole.Id, AssignedAtUtc = DateTime.UtcNow });
        _context.Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = adminId,
            CurrencyCode = "KES",
            Balance = 0,
            Status = WalletStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded admin user: {Email}", adminEmail);
    }
}
