using Finitech.Modules.IdentityAccess.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finitech.Modules.IdentityAccess.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for Identity and Access Management module
/// </summary>
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        // Seed system roles
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "System Administrator with full access",
                IsSystem = true
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "User",
                NormalizedName = "USER",
                Description = "Standard user",
                IsSystem = true
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Agent",
                NormalizedName = "AGENT",
                Description = "WalletFMCG Agent",
                IsSystem = true
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                Name = "Merchant",
                NormalizedName = "MERCHANT",
                Description = "Merchant for payments",
                IsSystem = true
            }
        );

        // Seed basic permissions
        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "wallet:read", Resource = "wallet", Action = "read" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "wallet:write", Resource = "wallet", Action = "write" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "banking:read", Resource = "banking", Action = "read" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Name = "banking:write", Resource = "banking", Action = "write" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Name = "payments:read", Resource = "payments", Action = "read" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000006"), Name = "payments:write", Resource = "payments", Action = "write" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000007"), Name = "admin:full", Resource = "admin", Action = "full" }
        );

        // Assign permissions to Admin role
        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { RoleId = Guid.Parse("00000000-0000-0000-0000-000000000001"), PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000007") },
            new RolePermission { RoleId = Guid.Parse("00000000-0000-0000-0000-000000000002"), PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000001") },
            new RolePermission { RoleId = Guid.Parse("00000000-0000-0000-0000-000000000002"), PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000002") },
            new RolePermission { RoleId = Guid.Parse("00000000-0000-0000-0000-000000000002"), PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000003") }
        );
    }
}
