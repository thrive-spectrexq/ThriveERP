using ThriveERP.Domain.Common;
using ThriveERP.Domain.Enums;
using ThriveERP.Domain.Events;
using ThriveERP.Domain.Exceptions;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a sales order. Aggregate root with rich domain methods
/// that enforce state-machine transitions and invariants.
/// </summary>
public class SalesOrder : BaseEntity, IAggregateRoot
{
    private readonly List<SaleItem> _saleItems = [];

    /// <summary>Gets or sets the order number. Max 30 characters.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional customer identifier (FK).</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Gets or sets the warehouse identifier (FK).</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>Gets or sets the current order status. Defaults to Draft.</summary>
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    /// <summary>Gets or sets the subtotal before discounts and taxes. Uses decimal(18,2).</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Gets or sets the total discount amount. Defaults to 0. Uses decimal(18,2).</summary>
    public decimal DiscountTotal { get; set; }

    /// <summary>Gets or sets the total tax amount. Defaults to 0. Uses decimal(18,2).</summary>
    public decimal TaxTotal { get; set; }

    /// <summary>Gets or sets the grand total (Subtotal - DiscountTotal + TaxTotal). Uses decimal(18,2).</summary>
    public decimal GrandTotal { get; set; }

    /// <summary>Gets or sets the date the order was created.</summary>
    public DateTime OrderDate { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the associated customer.</summary>
    public Customer? Customer { get; set; }

    /// <summary>Gets or sets the associated warehouse.</summary>
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>Gets the line items on this order.</summary>
    public ICollection<SaleItem> SaleItems
    {
        get => _saleItems;
        set
        {
            _saleItems.Clear();
            if (value is not null)
            {
                _saleItems.AddRange(value);
            }
        }
    }

    /// <summary>Gets the invoices generated from this order.</summary>
    public ICollection<Invoice> Invoices { get; set; } = [];

    /// <summary>Gets the returns associated with this order.</summary>
    public ICollection<Return> Returns { get; set; } = [];

    /// <summary>
    /// Adds a line item to the order. Only allowed while the order is in Draft status.
    /// Recalculates the order totals after adding.
    /// </summary>
    /// <param name="item">The sale item to add.</param>
    /// <exception cref="InvalidOrderStateTransitionException">Thrown when adding items to a non-Draft order.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
    public void AddItem(SaleItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOrderStateTransitionException(
                Status.ToString(), "AddItem (only allowed in Draft)");
        }

        item.SalesOrderId = Id;
        item.LineTotal = (item.UnitPrice * item.Quantity) - item.DiscountAmount;
        _saleItems.Add(item);

        RecalculateTotals();
    }

    /// <summary>
    /// Transitions the order from Draft to Submitted and raises a <see cref="SalesOrderSubmittedEvent"/>.
    /// </summary>
    /// <exception cref="InvalidOrderStateTransitionException">Thrown when the order is not in Draft status.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the order has no line items.</exception>
    public void Submit()
    {
        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOrderStateTransitionException(Status.ToString(), OrderStatus.Submitted.ToString());
        }

        if (_saleItems.Count == 0)
        {
            throw new InvalidOperationException("Cannot submit a sales order with no items.");
        }

        RecalculateTotals();
        Status = OrderStatus.Submitted;

        AddDomainEvent(new SalesOrderSubmittedEvent(Id));
    }

    /// <summary>
    /// Voids the order (allowed from Draft or Submitted only) and raises a <see cref="SalesOrderVoidedEvent"/>.
    /// </summary>
    /// <exception cref="InvalidOrderStateTransitionException">
    /// Thrown when the order is in Invoiced, Paid, or already Voided status.
    /// </exception>
    public void Void()
    {
        if (Status is not (OrderStatus.Draft or OrderStatus.Submitted))
        {
            throw new InvalidOrderStateTransitionException(Status.ToString(), OrderStatus.Voided.ToString());
        }

        Status = OrderStatus.Voided;

        AddDomainEvent(new SalesOrderVoidedEvent(Id));
    }

    /// <summary>
    /// Recalculates subtotal and grand total from current line items.
    /// </summary>
    private void RecalculateTotals()
    {
        Subtotal = 0;
        DiscountTotal = 0;

        foreach (var item in _saleItems)
        {
            Subtotal += item.UnitPrice * item.Quantity;
            DiscountTotal += item.DiscountAmount;
        }

        GrandTotal = Subtotal - DiscountTotal + TaxTotal;
    }
}
