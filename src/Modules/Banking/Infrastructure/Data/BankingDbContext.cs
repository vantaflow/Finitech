using Finitech.Modules.Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finitech.Modules.Banking.Infrastructure.Data;

public class BankingDbContext : DbContext
{
    public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options) { }

    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("banking");

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AccountType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.HasIndex(e => e.PartyId);
            entity.HasIndex(e => new { e.Status, e.OpenedAt });
        });

        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CardToken).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CardNumberMasked).HasMaxLength(30).IsRequired();
            entity.Property(e => e.CardType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CardNetwork).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(e => e.CardToken).IsUnique();
            entity.HasIndex(e => e.AccountId);
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LoanNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(e => e.LoanNumber).IsUnique();
            entity.HasIndex(e => e.PartyId);
            entity.HasIndex(e => e.Status);
        });
    }
}
