namespace ThriveERP.Domain.Exceptions;

/// <summary>
/// Thrown when a stock operation would result in a negative quantity on hand.
/// </summary>
public class InsufficientStockException : Exception
{
    public Guid ProductId { get; }
    public Guid WarehouseId { get; }
    public decimal RequestedChange { get; }
    public decimal CurrentQuantity { get; }

    public InsufficientStockException(Guid productId, Guid warehouseId, decimal requestedChange, decimal currentQuantity)
        : base($"Insufficient stock for product '{productId}' in warehouse '{warehouseId}'. " +
               $"Current quantity: {currentQuantity}, requested change: {requestedChange}.")
    {
        ProductId = productId;
        WarehouseId = warehouseId;
        RequestedChange = requestedChange;
        CurrentQuantity = currentQuantity;
    }

    public InsufficientStockException() : base("Insufficient stock to complete the operation.") { }

    public InsufficientStockException(string message) : base(message) { }

    public InsufficientStockException(string message, Exception innerException)
        : base(message, innerException) { }
}
