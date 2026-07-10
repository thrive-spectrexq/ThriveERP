namespace ThriveERP.Domain.Entities;

using ThriveERP.Domain.Common;

public class PurchaseItem : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; } = 0;
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }
    public Product? Product { get; set; }
}
