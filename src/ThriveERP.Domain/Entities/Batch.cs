using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a product batch for expiry and lot tracking.
/// </summary>
public class Batch : BaseEntity
{
    /// <summary>Gets or sets the product identifier (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the batch number. Max 50 characters. Unique per product.</summary>
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional expiry date.</summary>
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the batch was received.</summary>
    public DateTime ReceivedAtUtc { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the associated product.</summary>
    public Product Product { get; set; } = null!;

    /// <summary>Gets the stock movements referencing this batch.</summary>
    public ICollection<StockMovement> StockMovements { get; set; } = [];
}
