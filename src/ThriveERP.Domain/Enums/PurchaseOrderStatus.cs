namespace ThriveERP.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a purchase order.
/// </summary>
public enum PurchaseOrderStatus
{
    Draft = 0,
    Ordered = 1,
    PartiallyReceived = 2,
    Received = 3,
    Cancelled = 4
}
