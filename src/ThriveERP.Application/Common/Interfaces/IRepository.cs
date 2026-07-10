using ThriveERP.Domain.Common;

namespace ThriveERP.Application.Common.Interfaces;

/// <summary>
/// Generic repository contract for basic CRUD operations on domain entities.
/// </summary>
/// <typeparam name="T">An entity type that derives from <see cref="BaseEntity"/>.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>Gets an entity by its unique identifier.</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Gets all non-deleted entities.</summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Adds a new entity to the store.</summary>
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    /// <summary>Marks an existing entity as modified.</summary>
    void Update(T entity);

    /// <summary>Performs a soft delete on the entity.</summary>
    void Delete(T entity);
}
