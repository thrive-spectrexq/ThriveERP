using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Events;

/// <summary>
/// Raised when a sales order is voided.
/// </summary>
/// <param name="SalesOrderId">The identifier of the voided sales order.</param>
public record SalesOrderVoidedEvent(Guid SalesOrderId) : IDomainEvent;
