using ThriveERP.Application.Common.Interfaces;

namespace ThriveERP.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
