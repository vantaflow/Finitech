using Finitech.BuildingBlocks.Infrastructure.Data;
using Finitech.Modules.Ledger.Domain;
using Microsoft.EntityFrameworkCore;

namespace Finitech.Modules.Ledger.Infrastructure.Data;

public class LedgerDbContext : FinitechDbContext
{
    public LedgerDbContext(DbContextOptions<LedgerDbContext> options) : base(options)
    {
    }

    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<AccountBalance> AccountBalances => Set<AccountBalance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.ToTable("LedgerEntries", "ledger");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.AccountId)
                .IsRequired();

            entity.Property(e => e.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(e => e.EntryType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.AmountMinorUnits)
                .IsRequired();

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Reference)
                .HasMaxLength(100);

            entity.Property(e => e.TransactionId);

            entity.Property(e => e.OriginalEntryId);

            entity.Property(e => e.EntryDate)
                .IsRequired();

            entity.Property(e => e.RunningBalance)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue(LedgerEntryStatus.Posted);

            entity.Property(e => e.IdempotencyKey)
                .HasMaxLength(100);

            // Index for querying entries by account
            entity.HasIndex(e => e.AccountId);

            // Index for querying by transaction
            entity.HasIndex(e => e.TransactionId);

            // Unique index for idempotency
            entity.HasIndex(e => e.IdempotencyKey)
                .IsUnique()
                .HasFilter("[IdempotencyKey] IS NOT NULL");

            // Composite index for common queries
            entity.HasIndex(e => new { e.AccountId, e.CurrencyCode, e.EntryDate });
        });

        modelBuilder.Entity<AccountBalance>(entity =>
        {
            entity.ToTable("AccountBalances", "ledger");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.AccountId)
                .IsRequired();

            entity.Property(e => e.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(e => e.BalanceMinorUnits)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.ReservedAmountMinorUnits)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.LastUpdatedAt)
                .IsRequired();

            entity.Property(e => e.Version)
                .IsRequired()
                .IsConcurrencyToken();

            // Unique constraint: one balance per account per currency
            entity.HasIndex(e => new { e.AccountId, e.CurrencyCode })
                .IsUnique();
        });
    }
}
