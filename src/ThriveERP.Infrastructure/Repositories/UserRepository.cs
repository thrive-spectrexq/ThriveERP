namespace ThriveERP.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Infrastructure.Data;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ThriveErpDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _context.Set<User>().FirstOrDefaultAsync(u => u.Username == username, ct);
    }
}
