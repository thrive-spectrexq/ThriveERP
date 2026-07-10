namespace ThriveERP.Domain.Common;

/// <summary>
/// Abstract base class for all domain entities.
/// Provides identity, soft-delete, audit columns, and domain event support.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Gets or sets the unique identifier for this entity.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets a value indicating whether this entity is soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets the UTC timestamp when this entity was created.</summary>
    public DateTime? CreatedAtUtc { get; set; }

    /// <summary>Gets or sets the identifier of the user who created this entity.</summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>Gets or sets the UTC timestamp when this entity was last modified.</summary>
    public DateTime? ModifiedAtUtc { get; set; }

    /// <summary>Gets or sets the identifier of the user who last modified this entity.</summary>
    public Guid? ModifiedByUserId { get; set; }

    /// <summary>Gets the collection of domain events raised by this entity.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Adds a domain event to the entity's event collection.</summary>
    /// <param name="domainEvent">The domain event to add.</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>Removes a domain event from the entity's event collection.</summary>
    /// <param name="domainEvent">The domain event to remove.</param>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>Clears all domain events from the entity.</summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
