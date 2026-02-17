using Microsoft.EntityFrameworkCore;
using VaultCore.Domain.Entities;

namespace VaultCore.Infrastructure.Data;

/// <summary>
/// Main EF Core DbContext for VaultCore.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete
        modelBuilder.Entity<Domain.Entities.User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Domain.Entities.Wallet>().HasQueryFilter(w => !w.IsDeleted);
        modelBuilder.Entity<Domain.Entities.Transaction>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Domain.Entities.Role>().HasQueryFilter(r => !r.IsDeleted);
    }
}
