namespace Finitech.Modules.IdentityAccess.Domain.Entities;

/// <summary>
/// Role entity for RBAC
/// </summary>
public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; } // Cannot be deleted if system role
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}

public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty; // e.g., "wallet", "banking"
    public string Action { get; set; } = string.Empty;   // e.g., "read", "write"
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
