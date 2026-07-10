using ThriveERP.Domain.Entities;

namespace ThriveERP.Application.Common.Interfaces;

/// <summary>
/// Repository contract for <see cref="Product"/> entities.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>Finds a product by its SKU.</summary>
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);

    /// <summary>Finds a product by its barcode.</summary>
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken ct = default);
}
