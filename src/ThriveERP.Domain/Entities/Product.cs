using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a product in the catalog.
/// </summary>
public class Product : BaseEntity, IAggregateRoot
{
    /// <summary>Gets or sets the stock-keeping unit. Max 50 characters.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional barcode. Max 50 characters.</summary>
    public string? Barcode { get; set; }

    /// <summary>Gets or sets the product name. Max 200 characters.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional product description. Max 1000 characters.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the optional category identifier.</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Gets or sets the optional brand identifier.</summary>
    public Guid? BrandId { get; set; }

    /// <summary>Gets or sets the cost price. Uses decimal(18,2).</summary>
    public decimal CostPrice { get; set; }

    /// <summary>Gets or sets the selling price. Uses decimal(18,2).</summary>
    public decimal SellingPrice { get; set; }

    /// <summary>Gets or sets whether batch tracking is enabled. Defaults to false.</summary>
    public bool TrackBatches { get; set; }

    /// <summary>Gets or sets the minimum stock level that triggers a reorder alert. Defaults to 0.</summary>
    public int ReorderThreshold { get; set; }

    /// <summary>Gets or sets whether the product is active. Defaults to true.</summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties

    /// <summary>Gets or sets the product category.</summary>
    public Category? Category { get; set; }

    /// <summary>Gets or sets the product brand.</summary>
    public Brand? Brand { get; set; }

    /// <summary>Gets the sale line items referencing this product.</summary>
    public ICollection<SaleItem> SaleItems { get; set; } = [];

    /// <summary>Gets the stock levels across warehouses.</summary>
    public ICollection<StockLevel> StockLevels { get; set; } = [];

    /// <summary>Gets the batches of this product.</summary>
    public ICollection<Batch> Batches { get; set; } = [];

    /// <summary>Gets the supplier associations for this product.</summary>
    public ICollection<ProductSupplier> ProductSuppliers { get; set; } = [];
}
