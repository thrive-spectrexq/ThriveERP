namespace ThriveERP.Application.Features.Purchasing;
using System;
using System.Collections.Generic;

public record PurchaseOrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid SupplierId { get; init; }
    public string SupplierName { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal TaxTotal { get; init; }
    public decimal GrandTotal { get; init; }
    public DateTime OrderDate { get; init; }
    public DateOnly? ExpectedDate { get; init; }
    public List<PurchaseItemDto> Items { get; init; } = new();
}
