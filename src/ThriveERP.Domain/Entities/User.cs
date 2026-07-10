using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a system user with authentication credentials and role assignment.
/// </summary>
public class User : BaseEntity, IAggregateRoot
{
    /// <summary>Gets or sets the unique username. Max 50 characters.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the BCrypt password hash. Max 200 characters.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's full name. Max 150 characters.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional email address. Max 150 characters.</summary>
    public string? Email { get; set; }

    /// <summary>Gets or sets the foreign key to the user's role.</summary>
    public Guid RoleId { get; set; }

    /// <summary>Gets or sets whether the user account is active. Defaults to true.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the UTC timestamp of the last successful login.</summary>
    public DateTime? LastLoginAtUtc { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the role assigned to this user.</summary>
    public Role Role { get; set; } = null!;
}
