using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents an invoice generated from a sales order.
/// </summary>
public class Invoice : BaseEntity
{
    /// <summary>Gets or sets the invoice number. Max 30 characters.</summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the sales order identifier (FK).</summary>
    public Guid SalesOrderId { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the invoice was issued.</summary>
    public DateTime IssuedAtUtc { get; set; }

    /// <summary>Gets or sets the optional payment due date.</summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>Gets or sets the total amount due. Uses decimal(18,2).</summary>
    public decimal AmountDue { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the associated sales order.</summary>
    public SalesOrder SalesOrder { get; set; } = null!;

    /// <summary>Gets the payments applied to this invoice.</summary>
    public ICollection<Payment> Payments { get; set; } = [];
}
