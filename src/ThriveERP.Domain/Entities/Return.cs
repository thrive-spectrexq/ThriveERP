using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a product return against a sales order.
/// </summary>
public class Return : BaseEntity
{
    /// <summary>Gets or sets the sales order identifier (FK).</summary>
    public Guid SalesOrderId { get; set; }

    /// <summary>Gets or sets the product identifier (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the quantity returned. Uses decimal(18,3).</summary>
    public decimal Quantity { get; set; }

    /// <summary>Gets or sets the refund amount. Uses decimal(18,2).</summary>
    public decimal RefundAmount { get; set; }

    /// <summary>Gets or sets the optional return reason. Max 250 characters.</summary>
    public string? Reason { get; set; }

    /// <summary>Gets or sets the identifier of the user who processed the return (FK).</summary>
    public Guid ProcessedByUserId { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the return was processed.</summary>
    public DateTime ProcessedAtUtc { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the associated sales order.</summary>
    public SalesOrder SalesOrder { get; set; } = null!;

    /// <summary>Gets or sets the returned product.</summary>
    public Product Product { get; set; } = null!;

    /// <summary>Gets or sets the user who processed the return.</summary>
    public User ProcessedByUser { get; set; } = null!;
}
