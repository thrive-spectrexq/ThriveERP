using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Events;

/// <summary>
/// Raised when a payment is recorded against an invoice or a customer account.
/// </summary>
/// <param name="PaymentId">The identifier of the payment.</param>
/// <param name="InvoiceId">The identifier of the invoice, if applicable.</param>
/// <param name="Amount">The payment amount.</param>
public record PaymentReceivedEvent(Guid PaymentId, Guid? InvoiceId, decimal Amount) : IDomainEvent;
