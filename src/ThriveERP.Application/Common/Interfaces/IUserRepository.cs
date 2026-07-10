using ThriveERP.Domain.Entities;

namespace ThriveERP.Application.Common.Interfaces;

/// <summary>
/// Repository contract for <see cref="User"/> entities.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>Finds a user by their unique username.</summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
}
