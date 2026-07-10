using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a product brand.
/// </summary>
public class Brand : BaseEntity
{
    /// <summary>Gets or sets the brand name. Max 100 characters.</summary>
    public string Name { get; set; } = string.Empty;

    // Navigation properties

    /// <summary>Gets the products under this brand.</summary>
    public ICollection<Product> Products { get; set; } = [];
}
