namespace ThriveERP.Application.Features.Purchasing;
using System;

public record PurchaseItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal QuantityOrdered { get; init; }
    public decimal QuantityReceived { get; init; }
    public decimal UnitCost { get; init; }
    public decimal LineTotal { get; init; }
}
