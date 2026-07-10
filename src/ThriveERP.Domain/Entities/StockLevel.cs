using ThriveERP.Domain.Common;
using ThriveERP.Domain.Exceptions;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents the current stock level for a product in a specific warehouse.
/// Enforces the invariant that quantity on hand cannot go negative.
/// </summary>
public class StockLevel : BaseEntity
{
    /// <summary>Gets or sets the product identifier (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the warehouse identifier (FK).</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>Gets or sets the current quantity on hand. Defaults to 0. Uses decimal(18,3).</summary>
    public decimal QuantityOnHand { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the associated product.</summary>
    public Product Product { get; set; } = null!;

    /// <summary>Gets or sets the associated warehouse.</summary>
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// Adjusts the quantity on hand by the specified change amount.
    /// Throws <see cref="InsufficientStockException"/> if the result would be negative.
    /// </summary>
    /// <param name="change">The quantity to add (positive) or remove (negative).</param>
    /// <exception cref="InsufficientStockException">Thrown when the adjustment would result in negative stock.</exception>
    public void AdjustQuantity(decimal change)
    {
        var newQuantity = QuantityOnHand + change;

        if (newQuantity < 0)
        {
            throw new InsufficientStockException(ProductId, WarehouseId, change, QuantityOnHand);
        }

        QuantityOnHand = newQuantity;
    }
}
