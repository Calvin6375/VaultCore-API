using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultCore.Domain.Entities;

namespace VaultCore.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Amount).HasPrecision(18, 4);
        builder.Property(t => t.CurrencyCode).HasMaxLength(3);
        builder.Property(t => t.IdempotencyKey).HasMaxLength(64);
        builder.HasIndex(t => t.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL");
        builder.HasIndex(t => t.WalletId);
        builder.HasIndex(t => new { t.WalletId, t.CreatedAtUtc });
        builder.Property(t => t.RowVersion).IsRowVersion();
        builder.HasOne(t => t.Wallet).WithMany(w => w.Transactions).HasForeignKey(t => t.WalletId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.CounterpartyWallet).WithMany().HasForeignKey(t => t.CounterpartyWalletId).OnDelete(DeleteBehavior.Restrict);
    }
}
