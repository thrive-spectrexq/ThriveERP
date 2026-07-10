using ThriveERP.Domain.Common;
using ThriveERP.Domain.Enums;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Records a single inventory movement (addition or removal) for audit and traceability.
/// </summary>
public class StockMovement : BaseEntity
{
    /// <summary>Gets or sets the product identifier (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the warehouse identifier (FK).</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>Gets or sets the optional batch identifier (FK).</summary>
    public Guid? BatchId { get; set; }

    /// <summary>Gets or sets the type of stock movement.</summary>
    public MovementType MovementType { get; set; }

    /// <summary>Gets or sets the quantity change (positive for additions, negative for removals). Uses decimal(18,3).</summary>
    public decimal QuantityChange { get; set; }

    /// <summary>Gets or sets the optional reference type (e.g., "SalesOrder", "PurchaseOrder"). Max 50 characters.</summary>
    public string? ReferenceType { get; set; }

    /// <summary>Gets or sets the optional reference identifier linking to the source document.</summary>
    public Guid? ReferenceId { get; set; }

    /// <summary>Gets or sets the optional reason for the movement. Max 250 characters.</summary>
    public string? Reason { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the movement occurred.</summary>
    public DateTime OccurredAtUtc { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the associated product.</summary>
    public Product Product { get; set; } = null!;

    /// <summary>Gets or sets the associated warehouse.</summary>
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>Gets or sets the optional associated batch.</summary>
    public Batch? Batch { get; set; }
}
