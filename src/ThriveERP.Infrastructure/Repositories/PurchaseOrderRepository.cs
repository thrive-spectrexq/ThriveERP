namespace ThriveERP.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class PurchaseOrderRepository : Repository<PurchaseOrder>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(ThriveErpDbContext context) : base(context)
    {
    }

    public override async Task<IReadOnlyList<PurchaseOrder>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Set<PurchaseOrder>()
            .Include(po => po.Supplier)
            .Include(po => po.Warehouse)
            .Include(po => po.PurchaseItems)
            .ToListAsync(ct);
    }
}
