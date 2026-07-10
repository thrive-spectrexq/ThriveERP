namespace ThriveERP.Domain.Entities;

using ThriveERP.Domain.Common;

public class SupplierInvoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid PurchaseOrderId { get; set; }
    public decimal AmountDue { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime ReceivedAtUtc { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }
}
