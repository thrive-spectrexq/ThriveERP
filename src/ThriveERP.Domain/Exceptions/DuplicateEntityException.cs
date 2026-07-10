namespace ThriveERP.Domain.Exceptions;

/// <summary>
/// Thrown when an entity with a conflicting unique constraint already exists.
/// </summary>
public class DuplicateEntityException : Exception
{
    public string EntityName { get; }
    public string ConflictingField { get; }

    public DuplicateEntityException(string entityName, string conflictingField)
        : base($"A '{entityName}' with the same '{conflictingField}' already exists.")
    {
        EntityName = entityName;
        ConflictingField = conflictingField;
    }

    public DuplicateEntityException() : base("A duplicate entity already exists.") { }

    public DuplicateEntityException(string message) : base(message) { }

    public DuplicateEntityException(string message, Exception innerException)
        : base(message, innerException) { }
}
