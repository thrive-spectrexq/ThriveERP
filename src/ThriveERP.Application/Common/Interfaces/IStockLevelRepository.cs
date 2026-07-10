using ThriveERP.Domain.Entities;

namespace ThriveERP.Application.Common.Interfaces;

/// <summary>
/// Repository contract for <see cref="StockLevel"/> entities.
/// </summary>
public interface IStockLevelRepository : IRepository<StockLevel>
{
    /// <summary>Finds the stock level for a specific product in a specific warehouse.</summary>
    Task<StockLevel?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken ct = default);
}
