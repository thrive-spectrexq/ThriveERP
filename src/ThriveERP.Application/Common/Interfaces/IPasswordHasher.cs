namespace ThriveERP.Application.Common.Interfaces;

/// <summary>
/// Provides password hashing and verification.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hashes a plain-text password.</summary>
    /// <param name="password">The plain-text password.</param>
    /// <returns>The hashed representation of the password.</returns>
    string Hash(string password);

    /// <summary>Verifies a plain-text password against a stored hash.</summary>
    /// <param name="password">The plain-text password.</param>
    /// <param name="hash">The stored password hash.</param>
    /// <returns><c>true</c> if the password matches the hash; otherwise <c>false</c>.</returns>
    bool Verify(string password, string hash);
}
