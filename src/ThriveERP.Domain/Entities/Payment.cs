using ThriveERP.Domain.Common;
using ThriveERP.Domain.Enums;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a payment received from a customer.
/// </summary>
public class Payment : BaseEntity
{
    /// <summary>Gets or sets the optional invoice identifier (FK).</summary>
    public Guid? InvoiceId { get; set; }

    /// <summary>Gets or sets the optional customer identifier (FK).</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Gets or sets the payment amount. Uses decimal(18,2).</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the payment method used.</summary>
    public PaymentMethod Method { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the payment was received.</summary>
    public DateTime PaidAtUtc { get; set; }

    /// <summary>Gets or sets the identifier of the user who received the payment (FK).</summary>
    public Guid ReceivedByUserId { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the associated invoice.</summary>
    public Invoice? Invoice { get; set; }

    /// <summary>Gets or sets the associated customer.</summary>
    public Customer? Customer { get; set; }

    /// <summary>Gets or sets the user who received the payment.</summary>
    public User ReceivedByUser { get; set; } = null!;
}
