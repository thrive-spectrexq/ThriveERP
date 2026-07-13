namespace ThriveERP.Infrastructure.Repositories;

using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Infrastructure.Data;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(ThriveErpDbContext context) : base(context)
    {
    }
}
