using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a customer who purchases goods or services.
/// </summary>
public class Customer : BaseEntity, IAggregateRoot
{
    /// <summary>Gets or sets the customer name. Max 150 characters.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional phone number. Max 30 characters.</summary>
    public string? Phone { get; set; }

    /// <summary>Gets or sets the optional email address. Max 150 characters.</summary>
    public string? Email { get; set; }

    /// <summary>Gets or sets the optional address. Max 300 characters.</summary>
    public string? Address { get; set; }

    /// <summary>Gets or sets the credit limit. Defaults to 0. Uses decimal(18,2).</summary>
    public decimal CreditLimit { get; set; }

    /// <summary>Gets or sets the current outstanding balance. Defaults to 0. Uses decimal(18,2).</summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>Gets or sets whether the customer is active. Defaults to true.</summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties

    /// <summary>Gets the sales orders placed by this customer.</summary>
    public ICollection<SalesOrder> SalesOrders { get; set; } = [];
}
