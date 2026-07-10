namespace ThriveERP.Domain.Entities;

using ThriveERP.Domain.Common;

public class Expense : BaseEntity
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid AccountId { get; set; }
    public string? Description { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public Guid RecordedByUserId { get; set; }

    public Account? Account { get; set; }
    public User? RecordedByUser { get; set; }
}
