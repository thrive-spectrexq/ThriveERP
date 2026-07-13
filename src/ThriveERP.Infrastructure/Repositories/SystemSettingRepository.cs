using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Infrastructure.Data;

namespace ThriveERP.Infrastructure.Repositories;

public class SystemSettingRepository : Repository<SystemSetting>, ISystemSettingRepository
{
    public SystemSettingRepository(ThriveErpDbContext context) : base(context)
    {
    }

    public Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        return _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
    }
}
