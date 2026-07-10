namespace ThriveERP.Application.Features.Auth;
public record LoginResultDto(Guid UserId, string Username, string FullName, string RoleName, List<string> Permissions);
