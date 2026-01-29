using Finitech.Modules.Wallet.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finitech.Modules.Wallet.Infrastructure.Data;

public class WalletDbContext : DbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }

    public DbSet<WalletAccount> WalletAccounts => Set<WalletAccount>();
    public DbSet<WalletBalance> WalletBalances => Set<WalletBalance>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<ScheduledPayment> ScheduledPayments => Set<ScheduledPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("wallet");

        modelBuilder.Entity<WalletAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WalletLevel).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasIndex(e => e.PartyId).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<WalletBalance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CurrencyCode).HasMaxLength(3);
            entity.HasIndex(e => new { e.WalletId, e.CurrencyCode }).IsUnique();
            entity.HasOne(e => e.Wallet).WithMany(w => w.Balances).HasForeignKey(e => e.WalletId);
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionType).HasMaxLength(50);
            entity.Property(e => e.CurrencyCode).HasMaxLength(3);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.ExternalReference).HasMaxLength(100);
            entity.HasIndex(e => e.WalletId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ExternalReference).IsUnique().HasFilter("[ExternalReference] IS NOT NULL");
            entity.HasOne(e => e.Wallet).WithMany(w => w.Transactions).HasForeignKey(e => e.WalletId);
        });

        modelBuilder.Entity<ScheduledPayment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaymentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(e => e.Frequency).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => new { e.WalletId, e.Status });
            entity.HasIndex(e => e.NextExecutionAt);
            entity.HasOne(e => e.Wallet).WithMany().HasForeignKey(e => e.WalletId);
        });
    }
}
