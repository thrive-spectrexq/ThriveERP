using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a supplier who provides goods to the business.
/// </summary>
public class Supplier : BaseEntity, IAggregateRoot
{
    /// <summary>Gets or sets the supplier name. Max 150 characters.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional phone number. Max 30 characters.</summary>
    public string? Phone { get; set; }

    /// <summary>Gets or sets the optional email address. Max 150 characters.</summary>
    public string? Email { get; set; }

    /// <summary>Gets or sets the optional address. Max 300 characters.</summary>
    public string? Address { get; set; }

    /// <summary>Gets or sets the current outstanding balance owed to the supplier. Defaults to 0. Uses decimal(18,2).</summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>Gets or sets whether the supplier is active. Defaults to true.</summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties

    /// <summary>Gets the purchase orders issued to this supplier.</summary>
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];

    /// <summary>Gets the product-supplier associations.</summary>
    public ICollection<ProductSupplier> ProductSuppliers { get; set; } = [];
}
