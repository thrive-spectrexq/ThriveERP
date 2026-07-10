namespace ThriveERP.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Infrastructure.Data;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ThriveErpDbContext context) : base(context) { }

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default) => _context.Products.FirstOrDefaultAsync(p => p.Sku == sku, ct);
    public Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken ct = default) => _context.Products.FirstOrDefaultAsync(p => p.Barcode == barcode, ct);
}
