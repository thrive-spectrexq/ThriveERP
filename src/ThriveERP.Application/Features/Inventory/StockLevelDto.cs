namespace ThriveERP.Application.Features.Inventory;

public record StockLevelDto 
{ 
    public Guid Id { get; init; } 
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public decimal QuantityOnHand { get; init; }
}
