using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultCore.Domain.Entities;

namespace VaultCore.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Description).HasMaxLength(200);
        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
