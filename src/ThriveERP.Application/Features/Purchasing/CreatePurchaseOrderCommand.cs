namespace ThriveERP.Application.Features.Purchasing;
using MediatR;
using System;
using System.Collections.Generic;

public record CreatePurchaseOrderCommand(
    Guid SupplierId,
    Guid WarehouseId,
    DateOnly? ExpectedDate,
    List<CreatePurchaseItemCommand> Items
) : IRequest<PurchaseOrderDto>;

public record CreatePurchaseItemCommand(
    Guid ProductId,
    decimal QuantityOrdered,
    decimal UnitCost
);
