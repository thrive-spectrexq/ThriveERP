namespace ThriveERP.Infrastructure;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ThriveErpDbContext _context;

    public UnitOfWork(ThriveErpDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
