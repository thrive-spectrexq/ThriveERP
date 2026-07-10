using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a single line item on a sales order.
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>Gets or sets the sales order identifier (FK).</summary>
    public Guid SalesOrderId { get; set; }

    /// <summary>Gets or sets the product identifier (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the quantity sold. Uses decimal(18,3).</summary>
    public decimal Quantity { get; set; }

    /// <summary>Gets or sets the unit price at time of sale. Uses decimal(18,2).</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Gets or sets the discount amount applied to this line. Defaults to 0. Uses decimal(18,2).</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Gets or sets the computed line total ((UnitPrice × Quantity) - DiscountAmount). Uses decimal(18,2).</summary>
    public decimal LineTotal { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the parent sales order.</summary>
    public SalesOrder SalesOrder { get; set; } = null!;

    /// <summary>Gets or sets the associated product.</summary>
    public Product Product { get; set; } = null!;
}
