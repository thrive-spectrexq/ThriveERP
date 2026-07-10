namespace ThriveERP.Domain.Enums;

/// <summary>
/// Represents the classification of a ledger account in single-entry accounting.
/// </summary>
public enum AccountType
{
    Asset = 0,
    Liability = 1,
    Income = 2,
    Expense = 3,
    Equity = 4
}
