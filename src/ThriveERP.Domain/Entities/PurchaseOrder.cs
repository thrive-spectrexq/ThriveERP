namespace ThriveERP.Domain.Entities;

using ThriveERP.Domain.Common;
using ThriveERP.Domain.Enums;

public class PurchaseOrder : BaseEntity, IAggregateRoot
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; } = 0;
    public decimal GrandTotal { get; set; }
    public DateTime OrderDate { get; set; }
    public DateOnly? ExpectedDate { get; set; }

    public Supplier? Supplier { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    public ICollection<SupplierInvoice> SupplierInvoices { get; set; } = new List<SupplierInvoice>();
}
