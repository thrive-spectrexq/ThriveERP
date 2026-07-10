namespace ThriveERP.Domain.Entities;

/// <summary>
/// Join table linking products to their suppliers with supplier-specific pricing.
/// Does not inherit BaseEntity — uses a composite primary key of (ProductId, SupplierId).
/// </summary>
public class ProductSupplier
{
    /// <summary>Gets or sets the product identifier (part of composite PK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the supplier identifier (part of composite PK).</summary>
    public Guid SupplierId { get; set; }

    /// <summary>Gets or sets the supplier's SKU for this product. Max 50 characters.</summary>
    public string? SupplierSku { get; set; }

    /// <summary>Gets or sets the last purchase price from this supplier. Uses decimal(18,2).</summary>
    public decimal? LastPurchasePrice { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the associated product.</summary>
    public Product Product { get; set; } = null!;

    /// <summary>Gets or sets the associated supplier.</summary>
    public Supplier Supplier { get; set; } = null!;
}
