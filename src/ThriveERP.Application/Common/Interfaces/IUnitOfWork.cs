namespace ThriveERP.Application.Common.Interfaces;

/// <summary>
/// Coordinates saving changes across multiple repositories in a single transaction.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>Persists all pending changes to the database.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
