using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Events;

/// <summary>
/// Raised when a product's stock level drops below its configured reorder threshold.
/// </summary>
/// <param name="ProductId">The identifier of the product.</param>
/// <param name="WarehouseId">The identifier of the warehouse.</param>
/// <param name="CurrentQuantity">The current quantity on hand.</param>
/// <param name="Threshold">The configured reorder threshold.</param>
public record StockLevelBelowThresholdEvent(
    Guid ProductId,
    Guid WarehouseId,
    decimal CurrentQuantity,
    int Threshold) : IDomainEvent;
