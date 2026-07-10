namespace ThriveERP.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a sales order.
/// </summary>
public enum OrderStatus
{
    Draft = 0,
    Submitted = 1,
    Invoiced = 2,
    Paid = 3,
    Voided = 4
}
