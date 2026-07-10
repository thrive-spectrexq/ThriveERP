namespace ThriveERP.Application.Common.Interfaces;

/// <summary>
/// Provides information about the currently authenticated user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Gets the unique identifier of the current user, or <c>null</c> if unauthenticated.</summary>
    Guid? UserId { get; }

    /// <summary>Gets the username of the current user.</summary>
    string? Username { get; }

    /// <summary>Gets a value indicating whether the current user is authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Determines whether the current user has the specified permission.</summary>
    /// <param name="permissionCode">The permission code to check.</param>
    bool HasPermission(string permissionCode);
}
