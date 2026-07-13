using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Domain.Entities;

namespace ThriveERP.Application.Common.Interfaces;

public interface ISystemSettingRepository : IRepository<SystemSetting>
{
    Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken ct = default);
}
