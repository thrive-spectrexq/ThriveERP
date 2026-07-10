namespace ThriveERP.Domain.Enums;

/// <summary>
/// Represents the type of a financial transaction.
/// </summary>
public enum TransactionType
{
    Sale = 0,
    Purchase = 1,
    Expense = 2,
    Refund = 3,
    ManualAdjustment = 4
}
