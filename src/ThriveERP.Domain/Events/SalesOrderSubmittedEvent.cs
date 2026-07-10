using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Events;

/// <summary>
/// Raised when a sales order transitions to Submitted status.
/// </summary>
/// <param name="SalesOrderId">The identifier of the submitted sales order.</param>
public record SalesOrderSubmittedEvent(Guid SalesOrderId) : IDomainEvent;
