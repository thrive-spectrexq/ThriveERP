namespace ThriveERP.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Infrastructure.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

public class StockLevelRepository : Repository<StockLevel>, IStockLevelRepository
{
    public StockLevelRepository(ThriveErpDbContext context) : base(context)
    {
    }

    public async Task<StockLevel?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken ct = default)
    {
        return await _context.Set<StockLevel>()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.WarehouseId == warehouseId, ct);
    }

    public override async Task<IReadOnlyList<StockLevel>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Set<StockLevel>()
            .Include(sl => sl.Product)
            .Include(sl => sl.Warehouse)
            .ToListAsync(ct);
    }
}
