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

    public async Task<List<(string CategoryName, decimal TotalSales)>> GetSalesByCategoryAsync(CancellationToken ct = default)
    {
        var query = from item in _context.Set<SaleItem>()
                    join so in _context.Set<SalesOrder>() on item.SalesOrderId equals so.Id
                    join p in _context.Set<Product>() on item.ProductId equals p.Id
                    join c in _context.Set<Category>() on p.CategoryId equals c.Id
                    where so.Status != ThriveERP.Domain.Enums.OrderStatus.Voided
                    group item by c.Name into g
                    select new { CategoryName = g.Key, TotalSales = g.Sum(x => x.LineTotal) };

        var result = await query.ToListAsync(ct);
        return result.Select(x => (x.CategoryName, x.TotalSales)).ToList();
    }
}
