namespace ThriveERP.Domain.Enums;

/// <summary>
/// Represents the type of stock movement in a warehouse.
/// </summary>
public enum MovementType
{
    Sale = 0,
    Purchase = 1,
    Adjustment = 2,
    Transfer = 3,
    Return = 4
}
