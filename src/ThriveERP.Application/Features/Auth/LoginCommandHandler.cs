using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThriveERP.Application.Features.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    public Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // For development/demo purposes, hardcode access if the database is not seeded.
        // In a full implementation, you would query the IUserRepository and IPasswordHasher.
        
        if (request.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) && request.Password == "admin")
        {
            return Task.FromResult(new LoginResultDto(
                Guid.NewGuid(),
                "admin",
                "Administrator",
                "Admin",
                new List<string> { "all" }
            ));
        }
        else if (request.Username.Equals("cashier", StringComparison.OrdinalIgnoreCase) && request.Password == "cashier")
        {
            return Task.FromResult(new LoginResultDto(
                Guid.NewGuid(),
                "cashier",
                "Store Cashier",
                "Cashier",
                new List<string> { "sales" }
            ));
        }

        throw new Exception("Invalid username or password.");
    }
}
