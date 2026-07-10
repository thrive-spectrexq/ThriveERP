namespace ThriveERP.Domain.Entities;

/// <summary>
/// Immutable append-only audit log entry. Does not use soft-delete or modify audit columns.
/// </summary>
public class AuditLog
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who performed the action.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the action performed. Max 100 characters.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the affected entity. Max 100 characters.</summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>Gets or sets the identifier of the affected entity.</summary>
    public Guid EntityId { get; set; }

    /// <summary>Gets or sets optional details about the action. Max 2000 characters.</summary>
    public string? Details { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the action occurred.</summary>
    public DateTime OccurredAtUtc { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the user who performed the action.</summary>
    public User User { get; set; } = null!;
}
