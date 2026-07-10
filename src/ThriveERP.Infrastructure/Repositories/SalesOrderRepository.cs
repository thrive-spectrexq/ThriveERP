namespace ThriveERP.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Infrastructure.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

public class SalesOrderRepository : Repository<SalesOrder>, ISalesOrderRepository
{
    public SalesOrderRepository(ThriveErpDbContext context) : base(context)
    {
    }

    public async Task<SalesOrder?> GetWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<SalesOrder>()
            .Include(so => so.Customer)
            .Include(so => so.Warehouse)
            .Include(so => so.SaleItems)
            .FirstOrDefaultAsync(so => so.Id == id, ct);
    }

    public async Task<string> GetNextOrderNumberAsync(CancellationToken ct = default)
    {
        var count = await _context.Set<SalesOrder>().CountAsync(ct);
        return $"SO-{(count + 1):D6}";
    }
}
