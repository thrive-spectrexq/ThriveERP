namespace ThriveERP.Domain.Enums;

/// <summary>
/// Represents the method used to make a payment.
/// </summary>
public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    BankTransfer = 2,
    MobileMoney = 3,
    StoreCredit = 4
}
