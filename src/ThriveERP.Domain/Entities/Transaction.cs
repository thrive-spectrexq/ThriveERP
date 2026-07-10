namespace ThriveERP.Domain.Entities;

using ThriveERP.Domain.Common;
using ThriveERP.Domain.Enums;

public class Transaction : BaseEntity
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Description { get; set; }
    public DateTime OccurredAtUtc { get; set; }

    public Account? Account { get; set; }
}
