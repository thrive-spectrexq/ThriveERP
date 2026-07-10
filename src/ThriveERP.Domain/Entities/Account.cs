namespace ThriveERP.Domain.Entities;

using ThriveERP.Domain.Common;
using ThriveERP.Domain.Enums;

public class Account : BaseEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal Balance { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
