using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a system permission that can be assigned to roles.
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>Gets or sets the unique permission code. Max 100 characters.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional description. Max 250 characters.</summary>
    public string? Description { get; set; }

    // Navigation properties

    /// <summary>Gets the role associations for this permission.</summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
