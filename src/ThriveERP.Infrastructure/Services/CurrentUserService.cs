using ThriveERP.Application.Common.Interfaces;

namespace ThriveERP.Infrastructure.Services;

/// <summary>
/// Provides information about the currently authenticated user.
/// Set after successful login from the Desktop layer.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
    public string? RoleName { get; set; }
    private List<string> _permissions = new();

    public void SetUser(Guid userId, string username, string roleName, List<string> permissions)
    {
        UserId = userId;
        Username = username;
        RoleName = roleName;
        _permissions = permissions;
    }

    public void Clear()
    {
        UserId = null;
        Username = null;
        RoleName = null;
        _permissions.Clear();
    }

    public bool HasPermission(string permissionCode)
    {
        return _permissions.Contains("all", StringComparer.OrdinalIgnoreCase) ||
               _permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
    }
}
