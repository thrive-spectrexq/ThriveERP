using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a physical warehouse or storage location.
/// </summary>
public class Warehouse : BaseEntity, IAggregateRoot
{
    /// <summary>Gets or sets the warehouse name. Max 100 characters.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional physical location. Max 300 characters.</summary>
    public string? Location { get; set; }

    /// <summary>Gets or sets whether this is the default warehouse. Defaults to false.</summary>
    public bool IsDefault { get; set; }

    // Navigation properties

    /// <summary>Gets the stock levels stored in this warehouse.</summary>
    public ICollection<StockLevel> StockLevels { get; set; } = [];
}
