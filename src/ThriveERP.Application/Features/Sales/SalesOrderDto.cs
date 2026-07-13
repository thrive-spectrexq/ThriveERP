namespace ThriveERP.Application.Features.Sales;

using System;
using ThriveERP.Application.Common.Mappings;
using ThriveERP.Domain.Entities;

public record SalesOrderDto : IMapFrom<SalesOrder>
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal TaxTotal { get; init; }
    public decimal DiscountTotal { get; init; }
    public decimal GrandTotal { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime OrderDate { get; init; }
    public System.Collections.Generic.List<SaleItemDto> Items { get; init; } = new();
}
