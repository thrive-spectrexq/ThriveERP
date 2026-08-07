using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Application.Common.Interfaces;

namespace ThriveERP.Application.Features.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRoleRepository _roleRepository;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _roleRepository = roleRepository;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        // Update last login timestamp
        user.LastLoginAtUtc = DateTime.UtcNow;
        _userRepository.Update(user);

        // Retrieve role and permissions
        var role = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken);
        string roleName = role?.Name ?? "User";

        // For Administrator role, grant all permissions
        var permissions = roleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase)
            ? new List<string> { "all" }
            : new List<string> { "sales" };

        return new LoginResultDto(
            user.Id,
            user.Username,
            user.FullName,
            roleName,
            permissions
        );
    }
}
