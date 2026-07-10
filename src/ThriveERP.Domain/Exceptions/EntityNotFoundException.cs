namespace ThriveERP.Domain.Exceptions;

/// <summary>
/// Thrown when a requested entity cannot be found.
/// </summary>
public class EntityNotFoundException : Exception
{
    public string? EntityName { get; }
    public Guid EntityId { get; }

    public EntityNotFoundException(string entityName, Guid entityId)
        : base($"Entity '{entityName}' with id '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public EntityNotFoundException() : base("The requested entity was not found.") { }

    public EntityNotFoundException(string message) : base(message) { }

    public EntityNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
