using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a security role that can be assigned to users.
/// </summary>
public class Role : BaseEntity, IAggregateRoot
{
    /// <summary>Gets or sets the role name. Max 50 characters.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional role description. Max 250 characters.</summary>
    public string? Description { get; set; }

    // Navigation properties

    /// <summary>Gets the users assigned to this role.</summary>
    public ICollection<User> Users { get; set; } = [];

    /// <summary>Gets the permissions granted to this role.</summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
