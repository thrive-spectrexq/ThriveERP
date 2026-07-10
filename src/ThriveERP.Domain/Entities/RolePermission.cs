namespace ThriveERP.Domain.Entities;

/// <summary>
/// Join table linking roles to permissions. Does not inherit BaseEntity —
/// uses a composite primary key of (RoleId, PermissionId).
/// </summary>
public class RolePermission
{
    /// <summary>Gets or sets the role identifier (part of composite PK).</summary>
    public Guid RoleId { get; set; }

    /// <summary>Gets or sets the permission identifier (part of composite PK).</summary>
    public Guid PermissionId { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the associated role.</summary>
    public Role Role { get; set; } = null!;

    /// <summary>Gets or sets the associated permission.</summary>
    public Permission Permission { get; set; } = null!;
}
